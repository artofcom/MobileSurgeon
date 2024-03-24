using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Core.Tween
	{

	public class ImageFader : MonoBehaviour
	{
		public enum FadeMode
		{
			FADE_IN,
			FADE_OUT
		}

		protected float totalFadeTime = 0f;
		protected bool fadeStarted = false;
		protected float fadeTimer = 0f;
		protected FadeMode fadeMode = FadeMode.FADE_OUT;
		protected float currentAlpha = 1.0f;

		List<SpriteRenderer> baseSprites = new List<SpriteRenderer>();
		List<TMP_Text> textSprites = new List<TMP_Text>();
		List<Image> mListImages = new List<Image>();
		Action mCallBackFinish = null;

		public void GetAlphableComponents()
		{
			baseSprites.Clear();
			textSprites.Clear();

			if (gameObject != null)
			{
				SpriteRenderer baseSprite = gameObject.GetComponent<SpriteRenderer>();
				if (baseSprite != null)
				{
					baseSprites.Add(baseSprite);
				}
				TMP_Text textMesh = gameObject.GetComponent<TMP_Text>();
				if (textMesh != null)
				{
					textSprites.Add(textMesh);
				}
				Image uxImage = gameObject.GetComponent<Image>();
				if (uxImage != null)
				{
					mListImages.Add(uxImage);
				}

				IList<SpriteRenderer> childBaseSprites = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
				foreach (var b in childBaseSprites)
				{
					baseSprites.Add(b);
				}
				IList<TMP_Text> childTextMeshes = gameObject.GetComponentsInChildren<TMP_Text>(true);
				foreach (var b in childTextMeshes)
				{
					textSprites.Add(b);
				}
				IList<Image> childImages = gameObject.GetComponentsInChildren<Image>(true);
				foreach (var b in childImages)
				{
					mListImages.Add(b);
				}
			}
		}


		void Start()
		{
			GetAlphableComponents();
		}


		void Update()
		{
			if (fadeStarted)
			{
				fadeTimer -= Time.deltaTime;
				if (fadeTimer <= 0)
				{
					fadeTimer = 0;
					fadeStarted = false;
					if (mCallBackFinish != null)
						mCallBackFinish.Invoke();
					mCallBackFinish = null;
				}
				switch (fadeMode)
				{
					case FadeMode.FADE_IN:
						currentAlpha = 1.0f - (fadeTimer / totalFadeTime);
						break;
					case FadeMode.FADE_OUT:
						currentAlpha = fadeTimer / totalFadeTime;
						break;
				}

				updateImages();
			}
		}


		private void updateImages()
		{
			foreach (SpriteRenderer sprite in baseSprites)
			{
				sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, currentAlpha);
			}

			foreach (TMP_Text text in textSprites)
			{
				text.color = new Color(text.color.r, text.color.g, text.color.b, currentAlpha);
			}

			foreach (Image image in mListImages)
			{
				image.color = new Color(image.color.r, image.color.g, image.color.b, currentAlpha);
			}

		}


		public void FadeInTo1(float time, Action finCallback = null)
		{
			fadeStarted = true;
			totalFadeTime = time;
			fadeTimer = time;
			fadeMode = FadeMode.FADE_IN;
			mCallBackFinish = finCallback;
		}

		public void FadeOutTo0(float time, Action finCallback = null)
		{
			fadeStarted = true;
			totalFadeTime = time;
			fadeTimer = time;
			fadeMode = FadeMode.FADE_OUT;
			mCallBackFinish = finCallback;
		}

	}
}