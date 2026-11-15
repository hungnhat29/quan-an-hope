import json, statistics
PHU=[('ca_chua','Cà chua','cà chua','Thái múi',1.5,1),('hanh_tay','Hành tây','hành tây','Thái lát',1.5,1),('rau_muong','Rau muống','rau muống','Nhặt, rửa',1.0,1),
     ('khoai_tay','Khoai tây','khoai tây','Gọt, thái',2.0,2),('nam_rom','Nấm rơm','nấm rơm','Rửa, bổ đôi',1.0,2),
     ('bong_cai','Bông cải xanh','bông cải','Tách bông',1.5,3),('ot_chuong','Ớt chuông','ớt chuông','Bỏ hạt, thái',1.5,3),
     ('bi_do','Bí đỏ','bí đỏ','Gọt, thái',2.0,4),('mang_tay','Măng tây','măng tây','Cắt khúc',1.0,4),('nam_dong_co','Nấm đông cô','nấm đông cô','Thái lát',1.0,4)]
# id, name, template, base, tool, method, prep, prep_s, steps (P = do phu), cook_s, unlock, group
CH=[('trung','Trứng gà','Trứng chiên {p}','Cơm','Chảo','chiên','Đập, đánh',0.5,['Dầu ăn','P','Trứng gà','Nước mắm'],8,1,'chinh'),
    ('heo','Thịt heo','Heo xào {p}','Cơm','Chảo','xào','Thái mỏng',1.5,['Dầu ăn','Tỏi','Thịt heo','P'],10,1,'chinh'),
    ('dau_phu','Đậu phụ','Đậu phụ sốt {p}','Cơm','Nồi','sốt','Cắt miếng',1.0,['Dầu ăn','P','Đậu phụ','Nước mắm'],12,1,'chinh'),
    ('bo','Thịt bò','Mì Ý bò xào {p}','Mì Ý','Chảo','xào','Thái mỏng',1.5,['Bơ','Tỏi','Thịt bò','P'],10,2,'chinh'),
    ('tom','Tôm','Bún xào tôm {p}','Bún','Chảo','xào','Bóc vỏ',2.0,['Dầu ăn','Tôm','P','Nước mắm'],10,2,'chinh'),
    ('ca_hoi','Cá hồi','Cá hồi áp chảo {p}','Cơm','Chảo','áp chảo','Lọc xương',2.5,['Bơ','Cá hồi','P','Muối'],14,3,'chinh'),
    ('muc','Mực','Mực xào {p}','Cơm','Chảo','xào','Làm sạch, khứa',2.5,['Dầu ăn','Tỏi','Mực','P'],8,3,'chinh'),
    ('ga','Gà ta','Gà nướng mật ong {p}','Bánh mì','Vỉ nướng','nướng','Chặt miếng',2.0,['Mật ong','Gà ta','P'],18,4,'chinh'),
    ('tom_hum','Tôm hùm','Tôm hùm nướng bơ tỏi {p}','Bánh mì','Vỉ nướng','nướng','Bổ đôi',3.0,['Bơ','Tỏi','Tôm hùm','P'],20,0,'dotpha'),
    ('wagyu','Bò Wagyu','Bò Wagyu áp chảo {p}','Cơm','Chảo','áp chảo','Cắt miếng dày',2.0,['Muối','Bò Wagyu','Bơ','P'],14,0,'dotpha'),
    ('ca_ngu','Cá ngừ vây xanh','Cá ngừ vây xanh tataki {p}','Cơm','Chảo','áp chảo nhanh','Lọc thăn',2.5,['Dầu ăn','Cá ngừ vây xanh','P','Nước tương'],8,0,'dotpha'),
    ('cua','Cua hoàng đế','Cua hoàng đế hấp {p}','Bún','Nồi','hấp','Tách mai',3.0,['Nước dùng','Cua hoàng đế','P','Hành lá'],20,0,'dotpha')]
OVERRIDE={('bo','ca_chua'):'Mì Ý bò sốt cà chua'}
STEP_S=0.5; PLATE_S=1.0; SERVE_S=1.5
out=[]
for c in CH:
    cid,cn,tpl,base,tool,meth,prep,prep_s,steps,cook,unlock,grp=c
    for p in PHU:
        pid,pn,pin,pprep,pprep_s,punlock=p
        name=OVERRIDE.get((cid,pid),tpl.format(p=pin))
        st=[pn if s=='P' else s for s in steps]
        r_unlock=max(unlock,punlock) if grp=='chinh' else punlock
        total=prep_s+pprep_s+STEP_S*len(st)+cook+PLATE_S+SERVE_S
        out.append(dict(id=f'{cid}__{pid}',name=name,group=grp,main=cn,side=pn,tool=tool,method=meth,base=base,steps=st,
                        prep={cn:[prep,prep_s],pn:[pprep,pprep_s]},cook_s=cook,burn_s=cook*2,warn_s=cook*1.5,unlock_round=r_unlock,total_s=round(total,1)))
json.dump(dict(_note='Sinh tu make_recipes.py. Mon = 1 nguyen lieu chinh (hoac dot pha) + 1 do phu + do kho (vo han).',
               pantry=['Dầu ăn','Bơ','Tỏi','Nước mắm','Nước tương','Muối','Mật ong','Nước dùng','Hành lá'],
               bases=['Cơm','Mì Ý','Bún','Bánh mì'],
               timing=dict(step_s=STEP_S,plate_s=PLATE_S,serve_s=SERVE_S,burn_factor=2.0,warn_factor=1.5,burn_from_round=2),
               phu=[dict(id=a,name=b,in_name=c,prep=d,prep_s=e,unlock_round=f) for a,b,c,d,e,f in PHU],
               chinh=[dict(id=a,name=b,template=c,base=d,tool=e,method=f,prep=g,prep_s=h,steps=i,cook_s=j,unlock_round=k,group=l) for a,b,c,d,e,f,g,h,i,j,k,l in CH],
               recipes=out),open('recipes.json','w'),ensure_ascii=False,indent=1)
for rd in (1,2,3,4):
    rs=[r for r in out if r['group']=='chinh' and r['unlock_round']<=rd]
    print('round',rd,'dishes',len(rs),'avg total s',round(statistics.mean(r['total_s'] for r in rs),1),'min',min(r['total_s'] for r in rs),'max',max(r['total_s'] for r in rs))
dp=[r for r in out if r['group']=='dotpha']; print('dotpha dishes',len(dp),'avg',round(statistics.mean(r['total_s'] for r in dp),1),'max',max(r['total_s'] for r in dp))
for c in CH: 
    st=[s for s in c[8]]; print(c[1],'|',c[2],'|',c[4],'|',' → '.join('(đồ phụ)' if s=='P' else s for s in st),'|',c[9],'s', '| total w/ phu1.5:', round(c[7]+1.5+0.5*len(st)+c[9]+2.5,1))
