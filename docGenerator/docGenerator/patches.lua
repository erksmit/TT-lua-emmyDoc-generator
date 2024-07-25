-- this file contains manual additions to theodocs.lua for functions that are not documented or not feasable to document (GUI)
---@meta

---@alias frame number
---@alias font number
---@alias bool boolean
---@alias int integer
---@alias float number
---@alias animationdraft draft

---@class Font
---@field BIG font
---@field DEFAULT font
---@field SMALL font
Font = {}

Icon = {}
NinePatch = {}

---@class GUI
---@field content gui (undocumented) only exists on dialogs
GUI = {}

-- (undocumented)
---@param x number
---@param y number
function GUI:setAlignment(x, y) end

-- (undocumented)
---@param r number
---@param g number
---@param b number
function GUI:setColor(r, g, b) end

-- (undocumented) Only exists on layouts.
---@return gui layout
---@nodiscard
function GUI:getFirstPart() end