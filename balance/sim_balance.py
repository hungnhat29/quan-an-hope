"""Mo phong can bang - Quan an cua Hope.
Chay: python3 sim_balance.py
In ti le thang/tien do cho 3 kieu nguoi choi: naive (chon so to), nocombo (choi tot, khong gom bo), smart (gom bo relic).
Moi con so doc tu balance.json va relics.json (cung thu muc)."""
import json, random, statistics, sys, os
HERE=os.path.dirname(os.path.abspath(__file__))
B=json.load(open(os.environ.get('BAL',os.path.join(HERE,'balance.json'))))
RL=json.load(open(os.environ.get('RELICS',os.path.join(HERE,'relics.json'))))
SET_BONUS={st:{int(k):v for k,v in th.items()} for st,th in B['set_bonus'].items()}
def rnd_tier(odds,r): 
    x=r.random()*sum(odds); a=0
    for t,o in enumerate(odds):
        a+=o
        if x<a: return t
    return len(odds)-1
def round_of(d):
    for k,(a,b) in B['rounds'].items():
        if a<=d<=b: return int(k)
    return 4
class P:
    def __init__(s,style,seed):
        s.style=style; s.r=random.Random(seed); s.money=0; s.tk=0; s.black=0; s.black_bought=0
        s.sauce=0; s.sauce_bought=0; s.machine=False; s.relics=[]; s.fridge=[]; s.target=None
    def fx(s):
        f={}
        for rr in s.relics:
            for k,v in rr['fx'].items(): f[k]=f.get(k,0)+v
        cnt={}
        for rr in s.relics:
            if rr['set']: cnt[rr['set']]=cnt.get(rr['set'],0)+1
        for st,n in cnt.items():
            for th,bon in SET_BONUS[st].items():
                if n>=th:
                    for k,v in bon.items(): f[k]=max(f.get(k,0),v) if k in('phu_min','chinh_min','bt_t5') else f.get(k,0)+v
        return f
    def setcount(s):
        cnt={}
        for rr in s.relics:
            if rr['set']: cnt[rr['set']]=cnt.get(rr['set'],0)+1
        return cnt
def pull(p,day,black=False):
    r=p.r; f=p.fx(); rd=round_of(day)
    key='black' if black else ('storm' if day==3 else 'r%d'%rd)
    odds=B['ingredient_odds'][key]; cs=B['crate_size']
    nphu=(cs['phu_storm'] if day==3 and not black else cs['phu'])+int(f.get('phu_size',0))
    nch=(cs['chinh_storm'] if day==3 and not black else cs['chinh'])+int(f.get('chinh_size',0))
    def gen(n,grp):
        out=[]
        for _ in range(max(1,n)):
            t=rnd_tier(odds,r)
            if t==0 and f.get(grp+'_reroll0'): t=rnd_tier(odds,r)
            if r.random()<f.get(grp+'_up',0): t=min(5,t+1)
            if r.random()<f.get(grp+'_jump4',0): t=max(t,4)
            t=max(t,int(f.get(grp+'_min',0))); t=min(5,t+int(f.get(grp+'_plus',0)))
            out.append(t)
        return out
    phu=gen(nphu,'phu'); ch=gen(nch,'chinh')
    btc=B['breakthrough']['base_chance']+f.get('bt_chance',0)
    if r.random()<btc:
        for _ in range(B['breakthrough']['items']+int(f.get('bt_items',0))):
            ch.append(5 if r.random()<B['breakthrough']['tier5_share']+f.get('bt_t5',0) else 4)
    return phu,ch
def cook(p,day,ch,phu):
    D=B['dish']; f=p.fx(); r=p.r
    S=D['w_chinh']*ch+D['w_phu']*(sum(phu)/len(phu))
    b=S+D['offset']+f.get('dish_center',0)-(D['blackout_penalty'] if day==8 else 0)
    lo=int(b); t=lo+(1 if r.random()<b-lo else 0); t=max(0,min(4,t))
    if r.random()<f.get('dish_ruin',0): t=0
    t=max(t,int(f.get('dish_floor',0)))
    return t
def sauce_roll(p,day,t):
    S=B['sauce']; o=dict(S['odds_late'] if day>=S['late_from_day'] else S['odds_early']); f=p.fx()
    sf=min(o['fail'],f.get('sauce_safe',0)*100); o['fail']-=sf; o['same']+=sf
    keys=['fail','same','up1','up2','legend']; x=p.r.random()*100; a=0
    for k in keys:
        a+=o[k]
        if x<a: break
    if k=='fail': return 0
    if k=='same': return t
    if k=='up1': return min(4,t+1)
    if k=='up2': return min(4,t+2)
    return 5
def machine_roll(p,t):
    o=B['machine']['odds']; x=p.r.random()*100; a=0
    for k in ['trash','down1','same','up1','up2','up3']:
        a+=o[k]
        if x<a: break
    return {'trash':-1,'down1':max(0,t-1),'same':t,'up1':min(4,t+1),'up2':min(4,t+2),'up3':min(4,t+3)}[k]
def required(E):
    for th,rq in B['customers']['required_tier']:
        if E>=th: return rq
    return 4
def customers(p,day):
    C=B['customers']; rd=round_of(day); n=C['per_day']['d14'] if day==14 else C['per_day'][str(rd)]
    out=[]
    for _ in range(n):
        bands=C['easy_bands'][str(rd)]; x=p.r.random(); a=0
        for lo,hi,w in bands:
            a+=w
            if x<a: break
        out.append(p.r.randint(lo,hi))
    return out
def pay(p,day,E,t,impatience):
    D=B['dish']; f=p.fx(); rd=round_of(day); base=D['base_price'][str(rd)]
    E=max(1,E-impatience); rq=required(E); dt=(4 if t==5 else t)-rq; m=D['price_mult'][t]
    if dt>=0:
        strict=1-E/100; tip=base*m*strict*(0.3+0.2*dt)*(1+f.get('tip',0))
        return (base*m+tip)*(1+f.get('income',0)),1
    if dt==-1: return 0.5*base*m,0
    return -0.3*base,-1
def offers(p):
    owned={x['id'] for x in p.relics}; pool=[x for x in RL if x['id'] not in owned]
    return p.r.sample(pool,min(3,len(pool)))
def run(style,seed,log=False):
    p=P(style,seed); r=p.r; reached=0; p.money=B['start']['money']; p.tk=B['start']['bonus_tickets']
    for day in range(1,B['days']+1):
        rd=round_of(day); f=p.fx()
        if str(day) in B['debt_days']:
            due=B['debt_days'][str(day)]
            if p.money<due: return dict(win=False,day=day,p=p)
            p.money-=due
        if day==22: return dict(win=True,day=22,p=p)
        p.tk+=B['tickets_per_day']+(1 if f.get('ticket_every3') and day%3==0 else 0)
        cust=customers(p,day)
        if day==21: cust=cust[:-1]+[B['customers']['boss_easy']]
        need_pulls=-(-len(cust)//max(1,(B['crate_size']['chinh']+int(f.get('chinh_size',0)))))
        # --- next debt reserve
        nxt=min([int(k) for k in B['debt_days'] if int(k)>=day] or [99]); reserve=B['debt_days'].get(str(nxt),0)
        # --- relics
        offs=offers(p) if day>=B['start']['relic_shop_from_day'] else []
        if style=='naive':
            aff=[o for o in offs if o['price']<=p.tk]
            if aff and r.random()<0.5:
                o=max(aff,key=lambda o:o['price']); p.tk-=o['price']; add_relic(p,o)
        else:
            budget=p.tk-max(0,need_pulls-1) if day>2 else p.tk-need_pulls+1
            cand=sorted(offs,key=lambda o:-score(p,o))
            for o in cand:
                if score(p,o)>0 and o['price']<=budget and o['price']<=p.tk:
                    p.tk-=o['price']; budget-=o['price']; add_relic(p,o)
        # --- shop (money)
        spare=p.money-reserve*(0.0 if style=='naive' else 0.0)
        if style=='naive':
            if day>=6 and p.black_bought<B['black_ticket']['max']:
                pr=B['black_ticket']['price']+B['black_ticket']['step']*p.black_bought
                if p.money-pr>reserve*1.1: p.money-=pr; p.black+=1; p.black_bought+=1
        else:
            if not p.machine and day>=5 and p.money-B['machine']['price']>reserve*1.0: p.money-=B['machine']['price']; p.machine=True
            while day>=B['sauce']['unlock_day'] and p.sauce_bought<B['sauce']['max']:
                pr=B['sauce']['price']+B['sauce']['step']*p.sauce_bought
                if p.money-pr>reserve*1.15 and p.sauce<4: p.money-=pr; p.sauce+=1; p.sauce_bought+=1
                else: break
            if day>=17:
                while p.black_bought<B['black_ticket']['max']:
                    pr=B['black_ticket']['price']+B['black_ticket']['step']*p.black_bought
                    if p.money-pr>reserve*1.1: p.money-=pr; p.black+=1; p.black_bought+=1
                    else: break
        # --- pulls
        pulls=p.tk if style=='naive' else min(p.tk,need_pulls+(1 if day in(20,21) else 0))
        phu=[];ch=[]
        for _ in range(pulls):
            a,b=pull(p,day); phu+=a; ch+=b
        p.tk-=pulls
        nb=p.black if style=='naive' else (p.black if day>=20 else 0)
        for _ in range(nb):
            a,b=pull(p,day,True); phu+=a; ch+=b
        p.black-=nb
        # --- cook
        dishes=[]
        if style!='naive': ch.sort(reverse=True); phu.sort(reverse=True)
        else: r.shuffle(ch); r.shuffle(phu)
        k=0
        for c in ch:
            if k>=len(phu): break
            take=phu[k:k+1]
            k+=1; dishes.append(cook(p,day,c,take))
        # sauce / machine
        if p.sauce:
            if style=='naive':
                for i in range(len(dishes)):
                    if p.sauce and r.random()<0.4: dishes[i]=sauce_roll(p,day,dishes[i]); p.sauce-=1
            else:
                need_top = day>=19 and not any(d>=4 for d,_ in p.fridge)
                for i in sorted(range(len(dishes)),key=lambda i:-dishes[i]):
                    if p.sauce and dishes[i]==3 and (need_top or rd>=3): dishes[i]=sauce_roll(p,day,dishes[i]); p.sauce-=1
        if p.machine:
            uses=B['machine']['uses_per_day']
            for i in range(len(dishes)):
                if uses and dishes[i]<=1: 
                    v=machine_roll(p,dishes[i]); uses-=1; dishes[i]=v
            dishes=[d for d in dishes if d>=0]
        dishes+= [d for d,_ in p.fridge]; p.fridge=[]
        # --- serve
        imp=0 if rd==1 else (8 if style=='naive' else 3)*(1-min(0.9,f.get('patience',0)))
        if style=='naive':
            for E in cust:
                if not dishes: break
                t=dishes.pop(0); v,_=pay(p,day,E,t,imp); p.money+=v
                if day==21 and E==B['customers']['boss_easy'] and t>=4: p.money+=B['boss_bonus']
        else:
            order=sorted(cust,key=lambda E:E)  # strict first
            if day<21: keep=[d for d in sorted(dishes,reverse=True) if d>=4][:1] if day>=17 else []
            else: keep=[]
            for d in keep: dishes.remove(d)
            dishes.sort(reverse=True)
            for E in order:
                if not dishes: break
                rq=required(max(1,E-imp))
                ok=[d for d in dishes if (4 if d==5 else d)>=rq]
                t=min(ok) if ok else max(dishes)
                dishes.remove(t); v,_=pay(p,day,E,t,imp); p.money+=v
                if day==21 and E==B['customers']['boss_easy'] and t>=4: p.money+=B['boss_bonus']
            dishes=keep+sorted(dishes,reverse=True)
        cap=B['fridge']['slots']+int(f.get('fridge',0))
        p.fridge=[(d,0) for d in sorted(dishes,reverse=True)[:cap]]
    return dict(win=True,day=22,p=p)
def add_relic(p,o):
    if len(p.relics)>=B['relic_cap']:
        worst=min(p.relics,key=lambda x:score(p,x,owned=True)); p.relics.remove(worst)
    p.relics.append(o)
def score(p,o,owned=False):
    if p.style=='naive': return o['price']
    if p.style=='nocombo':
        return {'F1':8,'F4':5,'F2':4,'F3':3,'F9':3,'F5':2,'F10':4}.get(o['id'],2)
    cnt=p.setcount()
    if o['set']:
        if p.target is None and not owned: p.target=o['set']
        return 10+cnt.get(o['set'],0)*3 if o['set']==p.target else 1
    return {'F1':8,'F4':5,'F2':4,'F3':3,'F9':3,'F5':2,'F10':4}.get(o['id'],0)
def stats(style,N=4000):
    res=[run(style,s) for s in range(N)]
    win=sum(x['win'] for x in res)/N
    prog=statistics.mean(min(x['day'],22)/22 for x in res)
    pass_d=[sum(1 for x in res if x['day']>d or x['win'])/N for d in (4,9,15,22)]
    combo=[x for x in res if max(list(x['p'].setcount().values())+[0])>=3]
    cw=sum(x['win'] for x in combo)/max(1,len(combo))
    nc=[x for x in res if max(list(x['p'].setcount().values())+[0])<2]
    global NOCOMBO; NOCOMBO=(len(nc)/N, sum(x['win'] for x in nc)/max(1,len(nc)))
    return win,prog,pass_d,len(combo)/N,cw
if __name__=='__main__':
    for st in ('naive','nocombo','smart'):
        w,pr,pd,cf,cw=stats(st)
        print(f"{st:6s} win={w:.2f} progress={pr:.2f} pass(d4,d9,d15,d22)={[round(x,2) for x in pd]} combo3+={cf:.2f} win|combo={cw:.2f}")
