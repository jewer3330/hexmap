---------------------------------------------------------------------
-- RealmOfSpirits (C) xiaoniutech, All Rights Reserved
-- Created by: zhaojun
-- Date: 2020-08-31 13:51:32
---------------------------------------------------------------------

-- To edit this template in: Data/Config/Template.lua
-- To disable this template, check off menuitem: Options-Enable Template File

---@class DungeonManager 副本内部管理
require('UnityInclude')
EasyTouch = CS.HedgehogTeam.EasyTouch.EasyTouch
local Component = require('Component/Component')
local DungeonManager = class('DungeonManager',Component)
DungeonManager.WalkType = {}
DungeonManager.WalkType.UnWalkable = 0
DungeonManager.WalkType.Walkable = 1

--格式：类型+参数
--1.怪物点，关卡id
--2.BOSS点，关卡id
--3.下一层入口，填-1
--4.出生点，填-1
--5.双向传送点，填对应传送点设施id
--10.道具解锁型阻碍，填对应解锁道具id
--11.设施控制型阻碍，填对应控制设施id
--12.控制设施，填对应设施控制型阻碍id

--101.选择功能，填选择功能id（头节点）
--102.复杂机关，填脚本id
DungeonManager.EventType = {}
DungeonManager.EventType.None = 0
DungeonManager.EventType.Monster=1
DungeonManager.EventType.Boss=2
DungeonManager.EventType.NextLevel=3
DungeonManager.EventType.Start=4
DungeonManager.EventType.TP=5
DungeonManager.EventType.Item = 10
DungeonManager.EventType.Trap = 11
DungeonManager.EventType.Control = 12
DungeonManager.EventType.Select = 101
DungeonManager.EventType.Script = 102

DungeonManager.BuildingType = {}
DungeonManager.BuildingType.Floor = 0
DungeonManager.BuildingType.Building = 1


DungeonManager.Globals = {}
DungeonManager.Globals.Offset = CS.UnityEngine.Vector3.up * 0.3
DungeonManager.Globals.Radius = 1.5
DungeonManager.Globals.Height = 2 * DungeonManager.Globals.Radius
DungeonManager.Globals.RowHeight = 1.5 * DungeonManager.Globals.Radius
DungeonManager.Globals.HalfWidth = math.sqrt((DungeonManager.Globals.Radius ^ 2) - (DungeonManager.Globals.Radius * 0.5) ^ 2)
DungeonManager.Globals.Width = 2 * DungeonManager.Globals.HalfWidth
DungeonManager.Globals.ExtraHeight = DungeonManager.Globals.Height - DungeonManager.Globals.RowHeight
DungeonManager.Globals.Edge = DungeonManager.Globals.RowHeight - DungeonManager.Globals.ExtraHeight



function DungeonManager:awake()
	
end

function DungeonManager:onLevelLoaded()
	WorldInstance.Managers.UIManager:dispose()
	self.__mapData = nil
	--{go = go,data = Dungeon1.lua 中的某一行}
	self.__hexes = {}
	self.__root = GameObject('root')
	self.__box = self.__root:AddComponent(typeof(CS.UnityEngine.BoxCollider))
	local mapdata = Table.duplicate[DuplicateManager.currentRecord:getEctypeID()]
	self:load(mapdata.mapid)
	
	self:regNetEvent()
end

function DungeonManager:regNetEvent()
	self.handler_onEvent    		= net_reg(PtcG2CNtf_EctypeFacilities.getType(),handler(self,self.onEvent))
	self.handler_onStateChange 		= net_reg(PtcG2CNtf_EctypeStateChange.getType(),handler(self,self.onStateChange))
	self.handler_onPreEvent    		= net_reg(PtcG2CAck_PreReportEctypeEvent.getType(),handler(self,self.onPreEvent))
	self.handler_onRecvPos  		= net_reg(PtcG2CAck_ReportECtypePos.getType(), handler(self,self.onRecvPos))
	self.handler_onBagInfo  		= net_reg(PtcG2CAck_GetEctypeBagInfo.getType(),handler(self,self.onBagInfo))
	self.handler_onUseItem 			= net_reg(PtcG2CAck_UseEctypeBagItem.getType(),handler(self,self.onUseItem))
	self.handler_onAllReward 		= net_reg(PtcG2CNtf_EctypeOverAllReward.getType(),handler(self,self.onAllReward))
	self.handler_onFirstAllReward 	= net_reg(PtcG2CNtf_EctypeFirstOverAllReward.getType(),handler(self,self.onFirstAllReward))
	self.handler_onEventList 		= net_reg(PtcG2CAck_GetEctypeEventList.getType(),handler(self,self.onEventList))
	self.handler_onItemsChange 		= net_reg(PtcG2CNtf_EctypeItemsChange.getType(),handler(self,self.onItemsChange))
	self.handler_onEventSelect 		= net_reg(PtcG2CAck_EctypeSelEctypeItem.getType(),handler(self,self.onEventSelect))
	self.handler_onMonster 		    = net_reg(PtcG2CAck_EctypeMonster.getType(),handler(self,self.onMonster))
	self.handler_onBoss 		    = net_reg(PtcG2CAck_EctypeBoss.getType(),handler(self,self.onBoss))
	self.handler_onBoxInfos 		= net_reg(PtcG2CAck_EctypeBoxInfo.getType(),handler(self,self.onBoxInfos))
	self.handler_onBoxSelect 		= net_reg(PtcG2CAck_EctypeBoxSel.getType(),handler(self,self.onBoxSelect))
	self.handler_onNextLevel 		= net_reg(PtcG2CAck_EctypeNextLayer.getType(),handler(self,self.onNextLevel))
	self.handler_onTP 				= net_reg(PtcG2CAck_EctypeDTPoint.getType(),handler(self,self.onTP))
	self.handler_onItemObstruct 	= net_reg(PtcG2CAck_EctypeItemObstruct.getType(),handler(self,self.onItemObstruct))
	self.handler_onCtrolFac 		= net_reg(PtcG2CAck_EctypeCtrlFac.getType(),handler(self,self.onCtrolFac))
	
	
end

function DungeonManager:unregNetEvent()
	net_unreg(self.handler_onRecvPos)
	net_unreg(self.handler_onEvent)
	net_unreg(self.handler_onPreEvent)
	net_unreg(self.handler_onBagInfo)
	net_unreg(self.handler_onUseItem)
	net_unreg(self.handler_onAllReward)
	net_unreg(self.handler_onFirstAllReward)
	net_unreg(self.handler_onEventList)
	net_unreg(self.handler_onStateChange)
	net_unreg(self.handler_onItemsChange)
	net_unreg(self.handler_onEventSelect)
	net_unreg(self.handler_onMonster)
	net_unreg(self.handler_onBoxInfos)
	net_unreg(self.handler_onBoxSelect)
	net_unreg(self.handler_onBoss)
	net_unreg(self.handler_onNextLevel)
	net_unreg(self.handler_onTP)
	net_unreg(self.handler_onItemObstruct)
	net_unreg(self.handler_onCtrolFac)
	self.events = nil
end


--PtcC2GReq_EctypeCtrlFac
function DungeonManager:sendCtrlFac(id,nodeID)
	log.info('[net][DungeonManager] sendCtrlFac()',id,nodeID)
	local proto = net_id(PtcC2GReq_EctypeCtrlFac.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setNodeId(nodeID)
	proto:setParaValue(id)
	net_send(proto)
end

--@PtcG2CAck_EctypeCtrlFac
function DungeonManager:onCtrolFac(event)
	log.info('[net][DungeonManager] onCtrolFac()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
	else
		DuplicateManager:message(ret)
	end
end

--PtcC2GReq_EctypeItemObstruct
function DungeonManager:sendItemObsturct(id,nodeID)
	log.info('[net][DungeonManager] sendItemObsturct()',id,nodeID)
	local proto = net_id(PtcC2GReq_EctypeItemObstruct.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setNodeId(nodeID)
	proto:setParaValue(id)
	net_send(proto)
end

--@PtcG2CAck_EctypeItemObstruct
function DungeonManager:onItemObstruct(event)
	log.info('[net][DungeonManager] onItemObstruct()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
	else
		DuplicateManager:message(ret)
	end
end


--@PtcG2CAck_EctypeDTPoint
function DungeonManager:onTP(event)
	log.info('[net][DungeonManager] onTP()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
		local id = msg:getToNodeId()
		self:onSetPosition(id)
	else
		DuplicateManager:message(ret)
	end
end

function DungeonManager:onSetPosition(id)
	if self.charactor then
		self.charactor.transform.position = self:idToVector3(id) + DungeonManager.Globals.Offset
		self.currentHex = self.__hexes[id].gocom
		if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.LookAt end
		self.targetHex = self.currentHex
		--self:playerMove(true)
		self.controller:SetDestination(self.targetHex.position + DungeonManager.Globals.Offset);
	end
end


--PtcC2GReq_EctypeDTPoint
function DungeonManager:sendTP(x,y,z,eventID,nodeID)
	log.info('[net][DungeonManager] sendTP()',x,y,z,eventID,nodeID)
	local proto = net_id(PtcC2GReq_EctypeDTPoint.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setFromNodeId(nodeID)
	
	local toid = self:getTPPositionID(DuplicateManager.currentRecord:getEctypeID(),z,eventID)
	proto:setToNodeId(toid)
	
	net_send(proto)
end

function DungeonManager:getTPPositionID(id,layer,eventID)
	for k, v in pairs(self.events[id]) do
		if v:getzPos() == layer then
			if eventID == v:geteventId() then
				return v:getNodeId()
			end
		end
	end
end

--@PtcG2CAck_EctypeBoxSel
function DungeonManager:onBoxSelect(event)
	log.info('[net][DungeonManager] onBoxSelect()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateBox' then
			WorldInstance.Managers.UIManager:Pop()
		end
	else
		DuplicateManager:message(ret)
	end
end

--PtcG2CAck_EctypeNextLayer
function DungeonManager:onNextLevel(event)
	log.info('[net][DungeonManager] onNextLevel()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
		WorldInstance.Managers.sceneManager:loadScene('Assets/Scenes/Dungeon.unity')
	else
		DuplicateManager:message(ret)
	end
end

--PtcC2GReq_EctypeBoxSel
function DungeonManager:sendBoxSelect(id,nodeID)
	log.info('[net][DungeonManager] sendBoxSelect()',id,nodeID)
	local proto = net_id(PtcC2GReq_EctypeBoxSel.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setNodeId(nodeID)
	proto:setParaValue(id)
	net_send(proto)
end
--PtcC2GReq_EctypeNextLayer
function DungeonManager:sendNextLevel(id,nodeID)
	log.info('[net][DungeonManager] sendNextLevel()',id,nodeID)
	local proto = net_id(PtcC2GReq_EctypeNextLayer.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setNodeId(nodeID)
	proto:setParaValue(id)
	net_send(proto)
end

--@PtcG2CAck_EctypeBoxInfo
function DungeonManager:onBoxInfos(event)
	log.info('[net][DungeonManager] onBoxInfos()')
	local msg = event.msg;
	local id = msg:getEctypeId()
	if id == self.bagId then
		self.boxlist = msg:getDropitemlist()
		WorldInstance.Managers.UIManager:push('DuplicateBox',self.clickTarget)
		EventSystem.SendEvent(EventEnums.Dungeon.BoxInfo,self.boxlist)
	else
		DuplicateManager:message('副本不一致')
	end
end

--@PtcG2CAck_EctypeBoss
function DungeonManager:onBoss(event)
	log.info('[net][DungeonManager] onBoss()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
	else
		DuplicateManager:message(ret)
	end
end


--@PtcG2CAck_EctypeSelEctypeItem
function DungeonManager:onMonster(event)
	log.info('[net][DungeonManager] onMonster()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
	else
		DuplicateManager:message(ret)
	end
end

--PtcC2GReq_EctypeBoxInfo
function DungeonManager:sendBoxInfo(nodeID)
	log.info('[net][DungeonManager] sendBoxInfo()',nodeID)
	local proto = net_id(PtcC2GReq_EctypeBoxInfo.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setNodeId(nodeID)
	net_send(proto)
end


function DungeonManager:sendEventSelect(id,nodeID)
	log.info('[net][DungeonManager] sendEventSelect()',id,nodeID)
	local proto = net_id(PtcC2GReq_EctypeSelEctypeItem.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setSelId(id)
	proto:setNodeId(nodeID)
	net_send(proto)
end
--PtcC2GReq_EctypeMonster
function DungeonManager:sendFightMonster(param,nodeID)
	log.info('[net][DungeonManager] sendFightMonster()',param,nodeID)
	local proto = net_id(PtcC2GReq_EctypeMonster.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setParaValue(param)
	proto:setNodeId(nodeID)
	net_send(proto)
end


--PtcC2GReq_EctypeBoss
function DungeonManager:sendFightBoss(param,nodeID)
	log.info('[net][DungeonManager] sendFightBoss()',param,nodeID)
	local proto = net_id(PtcC2GReq_EctypeBoss.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setParaValue(param)
	proto:setNodeId(nodeID)
	net_send(proto)
end

--@PtcG2CAck_EctypeSelEctypeItem
function DungeonManager:onEventSelect(event)
	log.info('[net][DungeonManager] onEventSelect()')
	local msg = event.msg;
	--local id = msg:getEctypeId()
	local ret = msg:getResult()
	--local select = msg:getEctypeItemId()
	if ret == ErrCodeEctype.ECSuccess then
		--log.info('todo select success')
		if WorldInstance.Managers.UIManager:getCurrentPopName() == 'DuplicateEventUI' then
			WorldInstance.Managers.UIManager:Pop()
		end
	else
		DuplicateManager:message(ret)
	end
end

--@PtcG2CNtf_EctypeItemsChange
function DungeonManager:onItemsChange(event)
	log.info('[net][DungeonManager] onItemsChange()')
	local msg = event.msg;
	local id = msg:getEctypeId()
	if self.bagId == id then
		local changeList = msg:getChangeItemLists()
		for k, v in pairs(changeList) do
			--@EctypeItemData
			local id = v:getEItemId()
			self.bagItems[id] = v
			self.bagItems[id].data = Table.duplicateprop[v:getEItemId()]
		end
	end
end

function DungeonManager:getBagItemCount(id)
	if self.bagItems then
		if self.bagItems[id] then
			return  self.bagItems[id]:getEItemCount()
		end
	end
	return 0
end


--@PtcG2CNtf_EctypeStateChange
function DungeonManager:onStateChange(event)
	log.info('[net][DungeonManager] onStateChange()')
	local msg = event.msg;
	local id = msg:getEctypeId()
	local nodeID = msg:getNodeId()
	--@type EctypeEventNodes
	local state = msg:getState()
	local current = self.events[id][nodeID]
	current:setstate(state)
	self:__onStateChange(current)
end

function DungeonManager:__destroyBuilding(hex)
	if hex.building then
		GameObject.Destroy(hex.building)
	end
end

function DungeonManager:__activeBuilding(hex,active)
	if hex.building then
		hex.building.gameObject:SetActive(active)
	end
end

function DungeonManager:__onStateChange(current)
	if current then
		local nodeID = current:getNodeId()
		local state = current:getstate()
		local hex = self.__hexes[nodeID]
		if hex then
			log.info(' todo change the event state',hex.building,state)
			if hex.luadata.eventType == DungeonManager.EventType.Monster then
				if state == 1 then
					self:__destroyBuilding(hex)
					hex.building = GameObject.CreatePrimitive(CS.UnityEngine.PrimitiveType.Cube)
					hex.building.transform.localPosition = self:idToVector3(hex.data.id) + DungeonManager.Globals.Offset -- todo
					hex.building.transform.parent = self.__root.transform
				elseif state == 2 then
					self:__destroyBuilding(hex)
				end
			elseif hex.luadata.eventType == DungeonManager.EventType.Boss then
				if state == 2 then
					self:__destroyBuilding(hex)
				end
			elseif hex.luadata.eventType == DungeonManager.EventType.NextLevel then
			elseif hex.luadata.eventType == DungeonManager.EventType.Start then
			elseif hex.luadata.eventType == DungeonManager.EventType.TP then
			elseif hex.luadata.eventType == DungeonManager.EventType.Item then
			elseif hex.luadata.eventType == DungeonManager.EventType.Trap then
					self:__activeBuilding(hex,state == 0)
			elseif hex.luadata.eventType == DungeonManager.EventType.Control then
			elseif hex.luadata.eventType == DungeonManager.EventType.Select then
				if state == 2 then
					self:__destroyBuilding(hex)
				end
			end
					
		end
	else
		log.error('node not find ',nodeID)
	end
end


--@PtcG2CAck_PreReportEctypeEvent
function DungeonManager:onPreEvent(event)
	log.info('[net][DungeonManager] onPreEvent()')
	local msg = event.msg;
	local ret = msg:getResult() 
	if ret == ErrCodeEctype.ECSuccess then
		log.info(' todo pre event success')
	else
		DuplicateManager:message(ret)
	end
	self.preEventLock = false
end

--@PtcC2GReq_PreReportEctypeEvent
function DungeonManager:sendPreEvent(nodeID,x,y,z)
	log.info('[net][DungeonManager] sendPreEvent()',nodeID,x,y,z)
	
	local proto = net_id(PtcC2GReq_PreReportEctypeEvent.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setNodeId(nodeID)
	proto:setxPos(x)
	proto:setyPos(y)
	proto:setzPos(z)
	net_send(proto)
	
	self.preEventLock = true
end


--@PtcC2GReq_GetEctypeEventList
function DungeonManager:sendGetEventList()
	log.info('[net][DungeonManager] sendGetEventList()')
	local proto = net_id(PtcC2GReq_GetEctypeEventList.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	net_send(proto)
end

--@PtcG2CAck_GetEctypeEventList
function DungeonManager:onEventList(event)
	log.info('[net][DungeonManager] onEventList()')
	local msg = event.msg;
	local result = msg:getResult()
	if result == ErrCodeEctype.ECSuccess then
		local id = msg:getEctypeId()
		local list = msg:geteventList()
		--@type EctypeEventNodes
		self.events = self.events or {}
		self.events[id] = self.events[id] or {}
		for k, v in pairs(list) do
			self.events[id][v:getNodeId()] = v
		end
		
		--self.events[id] = list
		local x = msg:getlastXPos()
		local y = msg:getlastYPos()
		self.layer = msg:getlastZPos()
		local nodeid = self:xyzToid(x,y,self.layer)
		
		self:initAllFloors(self.layer)
		self:initAllBuildings(self.layer)
		self:initStartInfo(nodeid)
		self:loadCharacter()
	else
		DuplicateManager:message(result)
	end

end

function DungeonManager:initStartInfo(nodeid)
	if self.__hexes[nodeid] then
		self.startInfo = self.__hexes[nodeid]
	end
end

--@EctypeEventNodes
function DungeonManager:getEventList()
	return self.events[DuplicateManager.currentRecord:getEctypeID()] or {}
end

--@PtcC2GReq_ReportECtypePos
function DungeonManager:sendPos(x,y,z,nodeID)
	log.info('[net][DungeonManager] sendPos()',x,y,z,nodeID)
	local proto = net_id(PtcC2GReq_ReportECtypePos.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setxPos(x)
	proto:setyPos(y)
	proto:setzPos(z)
	proto:setNodeId(nodeID)
	net_send(proto)
end

--@PtcC2GReq_GetEctypeBagInfo
function DungeonManager:sendBagList()
	log.info('[net][DungeonManager] sendBagList()')
	local proto = net_id(PtcC2GReq_GetEctypeBagInfo.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	
	net_send(proto)
end

--@PtcC2GReq_UseEctypeBagItem
function DungeonManager:sendUseItem(itemID)
	log.info('[net][DungeonManager] sendUseItem()',itemID)
	local proto = net_id(PtcC2GReq_UseEctypeBagItem.getType())
	proto:setEctypeId(DuplicateManager.currentRecord:getEctypeID())
	proto:setEctypeItemId(itemID)
	
	net_send(proto)
end

--@PtcG2CNtf_EctypeOverAllReward
function DungeonManager:onAllReward(event)
	log.info('[net][DungeonManager] onAllReward()')
	local msg = event.msg;
end

--@PtcG2CNtf_EctypeFirstOverAllReward
function DungeonManager:onFirstAllReward(event)
	log.info('[net][DungeonManager] onFirstAllReward()')
	local msg = event.msg;
end

--@PtcG2CAck_UseEctypeBagItem
function DungeonManager:onUseItem(event)
	log.info('[net][DungeonManager] onUseItem()')
	local msg = event.msg;
	local ret = msg:getResult()
	if ret == ErrCodeEctype.ECSuccess then
		local bagId = msg:getEctypeId()
		if self.bagId ~= bagId then
			DuplicateManager:message('副本背包信息不一致')
			return
		end
		local id = msg:getEctypeItemId()
		--log.info(' todo update item id',id)
		local count = self.bagItems[id]:getEItemCount()
		if count > 1 then
			count = count - 1
			self.bagItems[id]:setEItemCount(count)
		else
			self.bagItems[id] = nil
		end
		EventSystem.SendEvent(EventEnums.Dungeon.BagInfo)
		DuplicateManager:message('使用成功')
	else
		DuplicateManager:message(ret)
	end
end

--@PtcG2CAck_GetEctypeBagInfo
function DungeonManager:onBagInfo(event)
	log.info('[net][DungeonManager] onBagInfo()')
	local msg = event.msg;
	--@ErrCodeEctype
	local ret = msg:getResult() 
	if ret == ErrCodeEctype.ECSuccess then
		--@type EctypeItemData
		local data = msg:getBagRecord()
		self.bagItems = {}
		for k, v in pairs(data) do
			--@EctypeItemData
			local id = v:getEItemId()
			self.bagItems[id] = v
			self.bagItems[id].data = Table.duplicateprop[id]
		end
		self.bagId = msg:getEctypeId()
		
		EventSystem.SendEvent(EventEnums.Dungeon.BagInfo)
	else
		DuplicateManager:message(ret)	
	end
end

function DungeonManager:getBagInfo()
	if not self.bagItems then return {} end
	
	local ret = {}
	for k, v in pairs(self.bagItems) do
		--v.data = Table.duplicateprop[v:getEItemId()]
		if not v.data then
			log.error('[DungeonManager][getBagInfo] not find item id ',v.id)
		else
			if v.data.type == 100 then
				local temp = clone(v.data)
				temp.count = v:getEItemCount()
				table.insert(ret,temp)
			else
				for i = 1, v:getEItemCount() do
					local temp = clone(v.data)
					temp.count = 1
					table.insert(ret,temp)
				end
			end
		end
	end
	return ret;
end

--@PtcG2CNtf_EctypeFacilities
function DungeonManager:onEvent(event)
	log.info('[net][DungeonManager] onEvent()')
	local msg = event.msg;
	local id = msg:getEctypeId()
	--@EctypeEventNodes
	--local current = self.events[id]
	--if current then
	--log.info(' todo change the event state')
	--end
end

--@ErrCodeEctype
function DungeonManager:onRecvPos(event)
	log.info('[net][DungeonManager] onRecvPos()')
	local msg = event.msg;
	local errorCode = msg:getResult();
	if errorCode == ErrCodeEctype.ECSuccess then
		
	else
		log.error(errorCode) --todo
	end
end




function DungeonManager:combineMesh()
	CS.Util.CombineMesh(self.__root)
end

function DungeonManager:initHex(go,v)
	local hexTable = {go = go,data = v}
	self.__hexes[v.id] = hexTable

	go.transform.position = self:idToVector3(v.id)
	
	go.transform.parent = self.__root.transform
	--local com = go:AddComponent(typeof(CS.Hex))
	--com.data = v
	local data = CS.MapCellData(v)
	self.__hexes[v.id].gocom = data
	go.name = '('..v.x..','..v.y..')'
	data.position = go.transform.position
	if v.walkType == DungeonManager.WalkType.UnWalkable then
		data.cost = 9999 --不可到达
	elseif v.walkType == DungeonManager.WalkType.Walkable then
		data.cost = 1 --可到达
	end
	--
	if v.eventId ~= 0 then
		local luadata = Table.facilities[v.eventId]
		if luadata == nil then
			log.error('[net][DungeonManager] eventId not find in lua table',v.eventId)
		else
			local t = split(luadata.effect,',')
			local eventType = tonumber(t[1])
			luadata.eventType = eventType
			luadata.eventParam = tonumber(t[2])
			if eventType == DungeonManager.EventType.Start then
				self.startInfo = hexTable
			end
			hexTable.luadata = luadata
		end
	end
	--if v.eventId == DungeonManager.EventType.Start then
		--self.startInfo = hexTable
	--end
	GameObject.Destroy(go:GetComponent(typeof(CS.HexModel)))
	GameObject.Destroy(go:GetComponent(typeof(CS.HexBrush)))
	
end

-- data 是 {go,DungeonData}
function DungeonManager:initBuilding(go,data,netData)
	data.building = go
	data.netData = netData
	--data.building.transform:SetParent(data.go.transform,false);
	data.building.transform.localPosition = self:idToVector3(data.data.id) + DungeonManager.Globals.Offset -- todo
	go.transform.parent = self.__root.transform
	--local com = go:AddComponent(typeof(CS.HexBuilding))
	--com.hex = data.gocom
	GameObject.Destroy(go:GetComponent(typeof(CS.HexModel)))
	GameObject.Destroy(go:GetComponent(typeof(CS.HexBrush)))
end


function DungeonManager:load(id)
	self.__mapData = require('Dungeon/Dungeon'..id)
	local width = self.__mapData.width * DungeonManager.Globals.Width
	local height =self.__mapData.height * DungeonManager.Globals.RowHeight 
	self.__totalCount = self.__mapData.width * self.__mapData.height
	self.__box.center = Vector3(width * 0.5 - DungeonManager.Globals.HalfWidth * 0.5,-0.5,height * 0.5 -  DungeonManager.Globals.Radius * 0.75);
	self.__box.size = Vector3(width + DungeonManager.Globals.HalfWidth,1,height + DungeonManager.Globals.Radius * 0.5)
	
	self:sendGetEventList()
	self:sendBagList()
	self:initUI()
end

function DungeonManager:initUI()
	WorldInstance.Managers.UIManager:push('DuplicateMainUI')
end

function DungeonManager:initAllFloors(layer)
	local count = 0
	for _, v in pairs(self.__mapData.walkableNodes) do
		--need floor
		if v.res ~= '' and v.res ~= nil and v.z == layer then --层级一样才生成，不一样的层级不管
			GenRes(v.res,function(go)
					count = count + 1
					self:initHex(go,v)
					
					if count == self.__totalCount  then
						self:combineMesh()
					end
				end)
		end
	end
	self:initNeighbors()
end

function DungeonManager:initAllBuildings(layer)
	for _, v in pairs(self:getEventList()) do
		if layer == v:getzPos() then
			local ret = self.__hexes[v:getNodeId()]
			if ret ~= nil then --层级一样才那啥，不一样的层级不管
				if ret.luadata.eventType ~= DungeonManager.EventType.Start then
					GenRes(ret.data.buildingRes,function (go)
							self:initBuilding(go,ret,v)
							self:__onStateChange(v)
						end)
				end
			else
				log.error('error map')
			end
		end
	end
end

function DungeonManager:initNeighbors()
	for _, v in pairs(self.__hexes) do
		self:link(v)
	end
end

function DungeonManager:link(hex)	
	if hex.data.y%2 == 0  then
		self:linkNativeMethod(0, hex,self:offset2hex(hex,0, 1))
		self:linkNativeMethod(1, hex,self:offset2hex(hex,1, 0))
		self:linkNativeMethod(2, hex,self:offset2hex(hex,0, -1))
		self:linkNativeMethod(3, hex,self:offset2hex(hex,-1, -1))
		self:linkNativeMethod(4, hex,self:offset2hex(hex,-1, 0))
		self:linkNativeMethod(5, hex,self:offset2hex(hex,-1, 1))
	else
		self:linkNativeMethod(0, hex,self:offset2hex(hex,1, 1))
		self:linkNativeMethod(1, hex,self:offset2hex(hex,1, 0))
		self:linkNativeMethod(2, hex,self:offset2hex(hex,1, -1))
		self:linkNativeMethod(3, hex,self:offset2hex(hex,0, -1))
		self:linkNativeMethod(4, hex,self:offset2hex(hex,-1, 0))
		self:linkNativeMethod(5, hex,self:offset2hex(hex,0, 1))		
	end
	
end

function DungeonManager:linkNativeMethod(id,lhex,rhex)
	if rhex then
		lhex.neighbors = lhex.neighbors or {}
		table.insert(lhex.neighbors,rhex)
		lhex.gocom:Link(id,rhex.gocom)
	end
end

function DungeonManager:offset2hex(hex,offset_x,offset_y)
	local x = hex.data.x + offset_x
	local y = hex.data.y + offset_y
	if ((y < self.__mapData.height and y>=0) and (x < self.__mapData.width and x >= 0)) then
		local id = self:xyzToid(x,y,hex.data.z)
		return self.__hexes[id]
	else
		return nil
	end
end

function DungeonManager:xyzToid(x,y,z)
	local offset = z * self.__mapData.width * self.__mapData.height
	local id = offset + y * self.__mapData.width + x
	return  id
end

function DungeonManager:vector2Toid(v2)
	local id = v2.y * self.__mapData.width + v2.x
	return  id
end

function DungeonManager:idToVector3(id)
	return self:vector2ToVector3(Vector2(self.__hexes[id].data.x,self.__hexes[id].data.y))
end

function DungeonManager:vector2ToVector3(v2)
	--return CS.World.ToPixel(v2)
	return  DungeonManager.ToPixel(v2)
end

function DungeonManager:vector3Toxy(v3)
	--return CS.World.ToHex(v3)
	return DungeonManager.ToHex(v3)
end

function DungeonManager.ToHex(pos)

	local px = pos.x + DungeonManager.Globals.HalfWidth;
	local py = pos.z + DungeonManager.Globals.Radius;

	local gridX = math.floor(px / DungeonManager.Globals.Width);
	local gridY = math.floor(py / DungeonManager.Globals.RowHeight);

	local gridModX = math.abs(px % DungeonManager.Globals.Width);
	local gridModY = math.abs(py % DungeonManager.Globals.RowHeight);

	local gridTypeA = (gridY % 2) == 0;

	local resultY = gridY;
	local resultX = gridX;
	local m = DungeonManager.Globals.ExtraHeight / DungeonManager.Globals.HalfWidth;

	if  gridTypeA  then
		-- middle
		resultY = gridY;
		resultX = gridX;
		-- left
		if gridModY < (DungeonManager.Globals.ExtraHeight - gridModX * m) then
			resultY = gridY - 1;
			resultX = gridX - 1;
		-- right
		elseif  gridModY < (-DungeonManager.Globals.ExtraHeight + gridModX * m) then
			resultY = gridY - 1;
			resultX = gridX;	
		end			
	else
		if  gridModX >= DungeonManager.Globals.HalfWidth then
				if  gridModY < (2 * DungeonManager.Globals.ExtraHeight - gridModX * m) then
						-- Top
						resultY = gridY - 1;
						resultX = gridX;
				else
						-- Right
						resultY = gridY;
						resultX = gridX;
				end
		else
			if gridModY < (gridModX * m) then
					-- Top
					resultY = gridY - 1;
					resultX = gridX;
			else
					-- Left
					resultY = gridY;
					resultX = gridX - 1;
			end
		end
	end
	return resultX, resultY;
						
end
							
function DungeonManager.ToPixel(hc)
	local offsetX = 0
	if hc.y % 2 == 1 then
		 offsetX  =  DungeonManager.Globals.Width / 2;
	end
	local x = hc.x * DungeonManager.Globals.Width + offsetX
	return Vector3(x, 0, hc.y * 1.5 * DungeonManager.Globals.Radius)
end



function DungeonManager:onDestroy()
	self:dispose()
	WorldInstance.Managers.DungeonManager = nil
end

--[[
销毁函数
]]--
function DungeonManager:dispose()
	if self.__hexes then
		for k, v in pairs(self.__hexes) do
			if v.go ~= nil then
				GameObject.Destroy(v.go)
			end
			if v.building ~= nil then
				GameObject.Destroy(v.building)
			end
		end
	end
	if self.handler then
		EasyTouch.On_SimpleTap('-',self.handler)
	end
	self:unregNetEvent();
end

function DungeonManager:loadCharacter()
	GenCharactorRes('hero_female_001.prefab',function(go)
			self.charactor = go
			self.charactor.transform.position = self:getStartPosition() + DungeonManager.Globals.Offset
			self.controller = self.charactor:GetComponent(typeof(CS.CharactorState))
			self:loadCamera()
			--self:prepareCamera()
	end)
end

function DungeonManager:prepareCamera()
	if self.charactor then
		local camera = Camera.main 
		local com = camera.gameObject:AddComponent(typeof(CS.MouseOrbitImproved))
		com.target = self.charactor.transform
	end
	self:init()
end

function DungeonManager:getStartPosition()
	if self.startInfo then
		self.currentHex = self.startInfo.gocom
		return  self:idToVector3(self.startInfo.data.id) 
	end
end

function DungeonManager:loadCamera()
	GenCameraRes('DungeonCamera.prefab',function(go)
		local com = go:GetComponent(typeof(CS.DragCameraSmooth))
		local child = com.transform:GetChild(0)
		local offset = - child.transform.position
		com.transform.position = self.charactor.transform.position + offset
		com.Target = self.charactor.transform
		self.camera = go:GetComponent(typeof(CS.UnityEngine.Camera))
		self.cameraControl = com
		--com.mode = CS.DragCameraSmooth.Mode.LookAt
		self.cameraControl.xMax = self.__box.size.x
		self.cameraControl.zMax = self.__box.size.z - 5
		self.cameraControl.zMin = - 5
	end)
	self:init()
end

function DungeonManager:init()
	self.handler = handler(self,self.onTouchUp)
	EasyTouch.On_SimpleTap('+',self.handler)  
end

function DungeonManager:onTouchUp(gesture)
	local ray = self.camera:ScreenPointToRay(Vector3(gesture.position.x,gesture.position.y,0))
	local ret,info = CS.Util.Raycast(ray,1000)
	if ret then
		
		local x,y = self:vector3Toxy(info.point)
		local id = self:xyzToid(x,y,self.layer)
		local data = self.__hexes[id]
		
		self.clickTarget = data
	--if data.data.id	~= self.currentHex.data.id then
		self.route = CS.MapCellData.searchRoute(self.currentHex,data.gocom)
		if self.route == nil or self.route.Count == 0 then
			WorldInstance.Managers.UIManager:message('该地方不能到达')
		else
			if self.route.Count > 0 then --删除起点
				self.route:RemoveAt(0)
				self.preEventLock = false
			end
			--if self.route.Count ~= 0 then --立即寻路
				--self:moveNextHex()
			--end
		end
	--end
		
	end
end



function DungeonManager:update()
	if not self.controller then
		--if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.Free end
		return
	end
	
	if not self.route then
		--if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.Free end
		return
	end
	if self.preEventLock then
		--if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.Free end
		return
	end
	
	if not self.controller.IsMoving then
		self:moveNextHex()
	end
	
	
end

function DungeonManager:moveNextHex()
	local move = false
	if self.route and self.route.Count > 0 then
		self.targetHex = self.route[0]
		self.route:RemoveAt(0)
		move = true
	else
		self.route = nil
		self:triggerEvent()
		self.targetHex = nil
	end
	
	self:playerMove(move)
end

function DungeonManager:playerMove(move)
	if move then
		log.info('move ---->',self.targetHex.x ..','..self.targetHex.y)
		if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.LookAt end
		local hexdata = self.__hexes[self.targetHex.id]  --目的节点
		if hexdata then
			if hexdata.netData then --目的节点存在事件
				--目的节点不可走，并且事件为出事状态
				if hexdata.luadata.iswalkable == 0 and hexdata.netData:getstate() == 0 then
					--self:sendPreEvent(self.targetHex.id,self.targetHex.x,self.targetHex.y) --停住
					self.controller:PlayMove(false)
					self.clickTarget = hexdata
					self:preTriggerEvent()
					self.route = nil
					if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.Free end
				else
					self.controller:SetDestination(self.targetHex.position + DungeonManager.Globals.Offset ); --停在目的节点
					self.controller:PlayMove(true)
					self.currentHex = self.targetHex
					--if hexdata.luadata.eventType ~= DungeonManager.EventType.Start then
					--self:sendPreEvent(self.targetHex.id,self.targetHex.x,self.targetHex.y)
					--end
					self:sendPos(self.targetHex.x,self.targetHex.y,self.targetHex.z,self.targetHex.id)
				end
			else

				self:sendPos(self.targetHex.x,self.targetHex.y,self.targetHex.z,self.targetHex.id)
				self.controller:SetDestination(self.targetHex.position + DungeonManager.Globals.Offset);
				self.controller:PlayMove(true)
				self.currentHex = self.targetHex
			end
		end
		
	else
		if self.cameraControl then self.cameraControl.mode = CS.DragCameraSmooth.Mode.Free end
		self.controller:PlayMove(false)
	end
	
end


function DungeonManager:preTriggerEvent()
	self:__event(self.clickTarget,self.targetHex)
end

function DungeonManager:triggerEvent()
	self:__event(self.clickTarget,self.currentHex)
end

function DungeonManager:__event(clickTarget,currentHex)
	if not clickTarget  and not currentHex then return end
	if not clickTarget.netData then return end
	--判断目的节点是否是点击节点
	if clickTarget.gocom == currentHex then
		--EctypeEventNodes

		if clickTarget.luadata.eventType == DungeonManager.EventType.Select and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.Monster and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.Monster and clickTarget.netData:getstate() == 1 then
			self:sendBoxInfo(clickTarget.data.id)
		elseif self.clickTarget.luadata.eventType == DungeonManager.EventType.Boss and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.NextLevel and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.TP and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.Item and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.Trap and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.Control and clickTarget.netData:getstate() == 0 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		elseif clickTarget.luadata.eventType == DungeonManager.EventType.Control and clickTarget.netData:getstate() == 2 then
			WorldInstance.Managers.UIManager:push('DuplicateEventUI',clickTarget)
		end
	end
end

return DungeonManager