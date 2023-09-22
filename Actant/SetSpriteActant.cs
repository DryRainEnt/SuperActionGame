using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class SetSpriteActant : SingleActant
	{
		public Sprite sprite;
		public Vector2 offset;
	
		public override void Act(Actor actor, float progress, bool isFirstFrame = false)
		{
			base.Act(actor, progress, isFirstFrame);
			// Put your code here
			
			// Set the frame
			actor.SetSprite(sprite);
		}

		public override void OnGUI(Rect position, float scale, float progress) 
		{
			if (!sprite)
				return;
			
	
	#if UNITY_EDITOR
			float xPos = position.width / 2 + (-sprite.rect.width + sprite.pivot.x + offset.x * Constants.DefaultPPU) * scale;
			float yPos = position.height / 2 + (-sprite.rect.height + sprite.pivot.y + offset.y * Constants.DefaultPPU) * scale;

			Rect spriteRect = new Rect(xPos, yPos,
				sprite.rect.width * scale,
				sprite.rect.height * scale);
            
			// 스프라이트의 UV 좌표 계산
			Rect spriteUV = new Rect(sprite.textureRect.x / sprite.texture.width,
				sprite.textureRect.y / sprite.texture.height,
				sprite.textureRect.width / sprite.texture.width,
				sprite.textureRect.height / sprite.texture.height);

			// 스프라이트 표시
			GUI.DrawTextureWithTexCoords(spriteRect, sprite.texture, spriteUV);
	#endif
		}

		public override void CopyFrom(SingleActant actant)
		{
			base.CopyFrom(actant);
			if (actant is not SetSpriteActant setSpriteActant) return;
			
			sprite = setSpriteActant.sprite;
			offset = setSpriteActant.offset;
		}
	}
}
