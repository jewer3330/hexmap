--[[
For ZeroProject
TimerManager lua
@author sherlock(dengwenyi88@gmail.com)
Creation: 2020-06-15
]]
require "utility"
require "Utils/timer"
-------------------------------------------------------------------------------

TimerManager = singletonClass("TimerManager")
local self = TimerManager
function TimerManager.ctor()
	self._timeruid = 0
	self._timers = {}
end
TimerManager.Instance()

function TimerManager.init()
	self._profiler = {}
end

function TimerManager.update(deltaTime)
	for k , v in pairs(self._timers) do
		v:update(deltaTime)
		if v:isWork() == false then
			self._timers[k] = nil
		end
    end
end

-------------------------------------------------------------------------------
-- start the run-once timer
-- @function [parent=#LogicManager] startTimer
-- @param #function func
-- @param #number interval second
-- @return #number timerid
function TimerManager.startTimer(func,interval)
	return TimerManager.startTimerEX(func,interval,false)
end
-------------------------------------------------------------------------------
-- @function [parent=#LogicManager] startTimerEX
-- @param #function func
-- @param #number interval second
-- @param #boolean isrepeat
-- @return #number timerid
function TimerManager.startTimerEX(func,interval,isrepeat)
	self._timeruid = self._timeruid + 1
	local timer = Timer:new()
	timer:set(self._timeruid,func,interval,isrepeat)
	self._timers[self._timeruid] = timer
	return self._timeruid
end

function TimerManager.stopTimer(timerid)
	for k , v in pairs(self._timers) do
		if k == timerid then
			v:setStop()
			self._timers[k] = nil
			return
		end
	end
end