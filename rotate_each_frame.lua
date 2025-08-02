local spr = app.activeSprite
if not spr then
  return app.alert("No active sprite!")
end

app.transaction(function()
  for i = 1, #spr.frames do
    app.activeFrame = spr.frames[i]
    local cel = spr.cels[spr.frames[i]]

    if cel then
      local image = cel.image:clone()

      local tempSprite = Sprite(image.width, image.height)
      tempSprite.cels[1].image = image

      tempSprite:rotate(1)

      cel.image = tempSprite.cels[1].image:clone()

      tempSprite:close()
    end
  end
end)

app.refresh()
