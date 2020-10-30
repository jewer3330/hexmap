
local Registry = import("Component/Registry")

local GameObject = {}

function GameObject.extend(target)
    target.components_ = {}
    target.componentsList = {}
    function target:checkComponent(name)
        return self.components_[name] ~= nil
    end

    function target:addComponent(name, ...)
        if not self:checkComponent(name) then
            self.components_[name] = {}
        end
        local component = Registry.newObject(name, target.gameObject)
        component.gameObject = target.gameObject
        component.name = name
        --self.components_[name] = component
        table.insert(self.components_[name],component)
        component:bind_(self)
        component:awake(...)
		
        table.insert(self.componentsList, component)
        return component
    end

    function target:removeComponent(comp)
        --local component = self.components_[name]
        local tbl = self.components_[comp.name]
        for i = #tbl, 1, -1 do
            if tbl[i] == comp then
                table.remove(tbl, i)
                break
            end
        end

        for i = #self.componentsList, 1, -1 do
            if self.componentsList[i] == comp then
                table.remove(self.componentsList, i)
                break
            end
        end
        if comp then comp:unbind_() end
    end

    function target:getComponents(name)
        return self.components_[name]
    end

    function target:getAllComponents()
        return self.componentsList
    end

    function target : removeAllComponents()
        --print('remove all ')
        --for i, v in pairs(self.componentsList) do
            --self : removeComponent(v)
        --end
		for i = #self.componentsList, 1, -1 do
			local comp = self.componentsList[i]
			if comp then comp:unbind_() end
		end
		self.components_ = {}
		self.componentsList = {}
    end

    function target:getComponent(name)
        return self.components_[name][1]
    end

    return target
end

return GameObject
