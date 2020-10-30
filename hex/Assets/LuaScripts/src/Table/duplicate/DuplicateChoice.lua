local duplicatechoice = {
    [1000101] = {id = 1000101,condition = '1,20001',option = '装瓶泉水',effect = '1,20002|2,20001',nextoption = 1000102},
    [1000102] = {id = 1000102,condition = '0',option = '喝泉水',effect = '4,11001',nextoption = 0},
}
return duplicatechoice
