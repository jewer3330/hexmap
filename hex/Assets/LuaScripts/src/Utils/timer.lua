---
-- @module Timer

---
-- @field [parent=#global] #TimerClass
Timer = class("Timer")

---
-- @function [parent=#TimerClass] new
-- @return #Timer

---
-- @function [parent=#Timer] ctor
function Timer:ctor()
	self._id = 0
	self._interval = 0
	self._isrepeat = false
	self._func = nil
	self._passedTime = 0
	self._isWork = true
end
---
-- @function [parent=#Timer] set
function Timer:set(id,func,interval,isrepeat)
	self._id = id
	self._interval = interval
	self._func = func
	self._isrepeat = isrepeat
	self._passedTime = 0
	self._isWork = true
end
---
-- return the timerid
-- @function [parent=#Timer] getID
-- @return #number id
function Timer:getID()
	return self._id
end
---
-- @function [parent=#Timer] update
function Timer:update(deltaTime)
	self._passedTime = self._passedTime + deltaTime
	if self._isrepeat == true then
		if self._passedTime > self._interval then
			self._passedTime = self._passedTime - self._interval
			self._func()
			--local debuginfo = debug.getinfo(self._func)
			--Log("   debug source : "..debuginfo.source.."  line : "..debuginfo.linedefined)
		end
--		while self._passedTime > self._interval do
--			self._passedTime = self._passedTime - self._interval
--			self._func()
--			if self._interval == 0 then
--				break
--			end
--		end
	else
		if self._passedTime > self._interval then
			self._func()
			self._isWork = false
		end
	end
end
---
-- @function [parent=#Timer] isWork
-- @return #boolean iswork
function Timer:isWork()
	return self._isWork
end
---
-- @function [parent=#Timer] setStop
function Timer:setStop()
	self._isWork = false
end
