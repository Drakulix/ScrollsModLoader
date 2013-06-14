using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameReplay.Mod
{
public class UIListPopup : MonoBehaviour
{
    //private Texture2D bgBar;
    private float BOTTOM_MARGIN_EXTRA = (Screen.height * 0.047f);
    //private float bottomMargin = 0.08f;
    private GUIContent buttonLeftContent;
    private Rect buttonLeftRect;
    private GUIContent buttonRightContent;
    private Rect buttonRightRect;
	private IListCallback callback;
    private float cardHeight;
    private GUISkin cardListPopupBigLabelSkin;
    private GUISkin cardListPopupGradientSkin;
    private GUISkin cardListPopupLeftButtonSkin;
    private GUISkin cardListPopupSkin;
    private List<Item> cards;
    private float cardWidth;
    private bool clickableItems;
    //private float costIconHeight;
    //private float costIconSize;
    //private float costIconWidth;
    private float fieldHeight;
    private Rect innerBGRect;
    private Rect innerRect;
    private Texture itemButtonTexture;
    private float labelsWidth;
    private float labelX;
    private bool leftButtonEnabled;
    private bool leftButtonHighlighted;
    private bool leftHighlightable;
    private GUISkin lobbySkin;
    private Vector4 margins;
    private int maxCharsName;
    private int maxCharsRK;
    private float offX;
    private float opacity;
    private Rect outerRect;
    private bool rightButtonEnabled;
    private bool rightButtonHighlighted;
    private bool rightHighlightable;
    private float scrollBarSize = 20f;
    public Vector2 scrollPos;
    private bool selectable;
	private bool rightrightbutton;
    private List<Item> selectedCards = new List<Item>();
    private bool showFrame;

    public void Init(Rect screenRect, bool showFrame, bool selectable, List<Item> cards, IListCallback callback, GUIContent buttonLeftContent, GUIContent buttonRightContent, bool leftButtonEnabled, bool rightButtonEnabled, bool leftHighlightable, bool rightHighlightable, Texture itemButtonTexture, bool clickableItems, bool rightrightbutton)
    {
        this.showFrame = showFrame;
        this.selectable = selectable;
        this.cards = cards;
        this.callback = callback;
        this.buttonLeftContent = buttonLeftContent;
        this.buttonRightContent = buttonRightContent;
        this.leftButtonEnabled = leftButtonEnabled;
        this.rightButtonEnabled = rightButtonEnabled;
        this.itemButtonTexture = itemButtonTexture;
        this.leftHighlightable = leftHighlightable;
        this.rightHighlightable = rightHighlightable;
        this.clickableItems = clickableItems;
        if (showFrame)
        {
            this.margins = new Vector4(12f, 12f, 12f, 12f + this.BOTTOM_MARGIN_EXTRA);
        }
        else
        {
            this.margins = new Vector4(0f, 0f, 0f, 0f + this.BOTTOM_MARGIN_EXTRA);
        }
        this.outerRect = screenRect;
        this.innerBGRect = new Rect(this.outerRect.x + this.margins.x, this.outerRect.y + this.margins.y, this.outerRect.width - (this.margins.x + this.margins.z), this.outerRect.height - (this.margins.y + this.margins.w));
        float num = 0.005f * Screen.width;
        this.innerRect = new Rect(this.innerBGRect.x + num, this.innerBGRect.y + num, this.innerBGRect.width - (2f * num), this.innerBGRect.height - (2f * num));
        float height = this.BOTTOM_MARGIN_EXTRA - (0.01f * Screen.height);
        this.buttonLeftRect = new Rect(this.innerRect.x + (this.innerRect.width * 0.03f), this.innerBGRect.yMax + (height * 0.28f), this.innerRect.width * 0.45f, height);
        this.buttonRightRect = new Rect(this.innerRect.xMax - (this.innerRect.width * 0.48f), this.innerBGRect.yMax + (height * 0.28f), this.innerRect.width * 0.45f, height);
        float num3 = (((float) Screen.height) / ((float) Screen.width)) * 0.3f;
        this.fieldHeight = (this.innerRect.width - this.scrollBarSize) / ((1f / num3) + 1f);
        //this.costIconSize = this.fieldHeight;
        //this.costIconWidth = this.fieldHeight / 2f;
        //this.costIconHeight = (this.costIconWidth * 72f) / 73f;
        this.cardHeight = this.fieldHeight * 0.72f;
        this.cardWidth = (this.cardHeight * 100f) / 75f;
        this.labelX = (this.cardWidth * 1.45f);
        this.labelsWidth = ((this.innerRect.width - this.labelX) /*- this.costIconSize*/) - this.scrollBarSize;
        this.maxCharsName = (int) (this.labelsWidth / 8f);
        this.maxCharsRK = (int) (this.labelsWidth / 10f);
		this.rightrightbutton = rightrightbutton;
    }

    private void OnGUI()
    {
        GUI.depth = 15;
        GUI.skin = this.cardListPopupSkin;
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
        Rect position = new Rect(this.outerRect.x + this.offX, this.outerRect.y, this.outerRect.width, this.outerRect.height);
        Rect rect2 = new Rect(this.innerBGRect.x + this.offX, this.innerBGRect.y, this.innerBGRect.width, this.innerBGRect.height);
        Rect rect3 = new Rect(this.innerRect.x + this.offX, this.innerRect.y, this.innerRect.width, this.innerRect.height);
        Rect rect4 = new Rect(this.buttonLeftRect.x + this.offX, this.buttonLeftRect.y, this.buttonLeftRect.width, this.buttonLeftRect.height);
        Rect rect5 = new Rect(this.buttonRightRect.x + this.offX, this.buttonRightRect.y, this.buttonRightRect.width, this.buttonRightRect.height);
        if (this.showFrame)
        {
            GUI.Box(position, string.Empty);
        }
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
        GUI.Box(rect2, string.Empty);
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
        this.cardListPopupBigLabelSkin.label.fontSize = (int) (this.fieldHeight / 1.7f);
        this.cardListPopupSkin.label.fontSize = (int) (this.fieldHeight / 2.5f);
        this.scrollPos = GUI.BeginScrollView(rect3, this.scrollPos, new Rect(0f, 0f, this.innerRect.width - 20f, this.fieldHeight * this.cards.Count));
        int num = 0;
        Item card = null;
        foreach (Item card2 in this.cards)
        {
            if (!card2.selectable())
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.skin = this.cardListPopupGradientSkin;
			Rect rect6 = new Rect(2f, num * this.fieldHeight, (this.innerRect.width - this.scrollBarSize) - 2f, this.fieldHeight);
			if (rightrightbutton) {
				rect6.width = rect6.width - this.fieldHeight - 2f;
			}
            if ((rect6.yMax < this.scrollPos.y) || (rect6.y > (this.scrollPos.y + rect3.height)))
            {
                num++;
                GUI.color = Color.white;
            }
            else
            {
                if (this.clickableItems)
                {
                    if (GUI.Button(rect6, string.Empty))
                    {
						if (!rightrightbutton)
                        	this.callback.ItemClicked(this, card2);
						else {
							if (this.selectable)
							{
								if (!this.selectedCards.Contains(card2))
								{
									this.selectedCards.Add(card2);
								}
								else
								{
									this.selectedCards.Remove(card2);
								}
							}
							else
							{
								card = card2;
							}
						}
                    }
                }
                else
                {
                    GUI.Box(rect6, string.Empty);
                }
                Texture image = card2.getImage();
                if (image != null)
                {
                    GUI.DrawTexture(new Rect((this.fieldHeight * 0.21f), (num * this.fieldHeight) + ((this.fieldHeight - this.cardHeight) * 0.43f), this.cardWidth, this.cardHeight), image);
                }
                GUI.skin = this.cardListPopupBigLabelSkin;
                string text = card2.getName();
                Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(text));
                Rect rect7 = new Rect(this.labelX, ((num * this.fieldHeight) - 3f) + (this.fieldHeight * 0.01f), this.labelsWidth, this.cardHeight);
                GUI.Label(rect7, (vector.x >= rect7.width) ? (text.Substring(0, Mathf.Min(text.Length, this.maxCharsName)) + "...") : text);
                GUI.skin = this.cardListPopupSkin;
				string str4 = card2.getDesc ();
                Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(str4));
                Rect rect8 = new Rect(this.labelX, ((num * this.fieldHeight) - 3f) + (this.fieldHeight * 0.57f), this.labelsWidth, this.cardHeight);
                GUI.Label(rect8, (vector2.x >= rect8.width) ? (str4.Substring(0, Mathf.Min(str4.Length, this.maxCharsRK)) + "...") : str4);
                //this.RenderCost(new Rect(((this.labelX + this.labelsWidth) + ((this.costIconSize - this.costIconWidth) / 2f)) - 5f, (num * this.fieldHeight) + ((this.fieldHeight - this.costIconHeight) / 2f), this.costIconWidth, this.costIconHeight), card2);
                GUI.skin = this.cardListPopupLeftButtonSkin;
                Rect rect9 = new Rect(0f, num * this.fieldHeight, this.fieldHeight, this.fieldHeight);
				Rect rect10 = new Rect (((this.innerRect.width - this.scrollBarSize)), num * this.fieldHeight, this.fieldHeight, this.fieldHeight);
                if ((this.itemButtonTexture == null) && !this.selectable)
                {
                    GUI.enabled = false;
                }
                /*if (GUI.Button(rect9, string.Empty) && card2.selectable())
                {
					if (this.selectable && !rightrightbutton)
                    {
                        if (!this.selectedCards.Contains(card2))
                        {
                            this.selectedCards.Add(card2);
                        }
                        else
                        {
                            this.selectedCards.Remove(card2);
                        }
                    }
                    else
                    {
                        card = card2;
                    }
                    App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                }*/
				if (rightrightbutton) {
					if (GUI.Button(rect10, string.Empty) && card2.selectable())
					{
						callback.ButtonClicked (this, ECardListButton.BUTTON_RIGHT);
						App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
					}
				}
                if ((this.itemButtonTexture == null) && !this.selectable)
                {
                    GUI.enabled = true;
                }
                if (card2.selectable())
                {
                    /*if (this.selectable)
                    {
                        if (this.selectedCards.Contains(card2))
                        {
                            GUI.DrawTexture(rect9alt, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                        }
                        else
                        {
                            GUI.DrawTexture(rect9alt, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                        }
                    }
                    else if (this.itemButtonTexture != null)
                    {
                        GUI.DrawTexture(rect9, this.itemButtonTexture);
                    }*/
					if (rightrightbutton) {
						Texture2D arrow = ResourceManager.LoadTexture ("chatUI/arrow_");
						GUIUtility.RotateAroundPivot (90, rect9.center);
						GUI.DrawTexture(new Rect(rect9.center.x-rect9.width/4.0f-2.0f, rect9.center.y-rect9.height/4.0f-1.0f, rect9.width/2.0f, rect9.height/2.0f), arrow);
						GUI.matrix = Matrix4x4.identity;
						
						Texture2D arrow2 = ResourceManager.LoadTexture ("chatUI/arrow_");
						GUIUtility.RotateAroundPivot (270, rect10.center);
						GUI.DrawTexture(new Rect(rect10.center.x-rect10.width/4.0f+2.0f, rect10.center.y-rect10.height/4.0f-1.0f, rect10.width/2.0f, rect10.height/2.0f), arrow2);
						GUI.matrix = Matrix4x4.identity;
					}
                }
                if (!card2.selectable())
                {
                    GUI.color = Color.white;
                }
                num++;
            }
        }
        GUI.EndScrollView();
        if (card != null)
        {
            this.callback.ItemButtonClicked(this, card);
        }
        GUI.skin = this.lobbySkin;
        if (this.buttonLeftContent != null)
        {
            if (!this.leftButtonEnabled)
            {
                GUI.enabled = false;
            }
            if (GUI.Button(rect4, this.buttonLeftContent))
            {
                if (this.selectable)
                {
                    this.callback.ButtonClicked(this, ECardListButton.BUTTON_LEFT, new List<Item>(this.selectedCards));
                    this.selectedCards.Clear();
                }
                else
                {
                    this.callback.ButtonClicked(this, ECardListButton.BUTTON_LEFT);
                }
                App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
            }
            Rect rect10 = new Rect(rect4.x + (rect4.height * 0.01f), rect4.y, rect4.height, rect4.height);
			if (this.leftButtonHighlighted)
            {
                GUI.DrawTexture(rect10, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else if (this.leftHighlightable)
            {
                GUI.DrawTexture(rect10, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }
            GUI.Label(rect4, this.buttonLeftContent);
            if (!this.leftButtonEnabled)
            {
                GUI.enabled = true;
            }
        }
        if (this.buttonRightContent != null)
        {
            if (!this.rightButtonEnabled)
            {
                GUI.enabled = false;
            }
            if (GUI.Button(rect5, this.buttonRightContent))
            {
                if (this.selectable)
                {
                    this.callback.ButtonClicked(this, ECardListButton.BUTTON_RIGHT, new List<Item>(this.selectedCards));
                    this.selectedCards.Clear();
                }
                else
                {
                    this.callback.ButtonClicked(this, ECardListButton.BUTTON_RIGHT);
                }
                App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
            }
            Rect rect11 = new Rect(rect5.x + (rect5.height * 0.01f), rect5.y, rect5.height, rect5.height);
            if (this.rightButtonHighlighted)
            {
                GUI.DrawTexture(rect11, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else if (this.rightHighlightable)
            {
                GUI.DrawTexture(rect11, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }
            GUI.Label(rect5, this.buttonRightContent);
            if (!this.rightButtonEnabled)
            {
                GUI.enabled = false;
            }
        }
    }

    private void RenderCost(Rect rect, Item card)
    {
        int num = 0;
        Texture image = null;
        /*if (card.getCostOrder() > 0)
        {
            num = card.getCostOrder();
            image = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        }
        else if (card.getCostEnergy() > 0)
        {
            num = card.getCostEnergy();
            image = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        }
        else if (card.getCostDecay() > 0)
        {
            num = card.getCostDecay();
            image = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");
        }
        else if (card.getCostGrowth() > 0)
        {
            num = card.getCostGrowth();
            image = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        }*/
        if (image != null)
        {
            GUI.DrawTexture(rect, image);
            char[] chArray = Convert.ToString(num).ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                Rect position = new Rect((rect.xMax + 5f) - (((chArray.Length - i) * rect.height) * 0.6f), rect.y + 1f, rect.height * 0.7f, rect.height);
                Texture texture2 = ResourceManager.LoadTexture("Scrolls/yellow_" + chArray[i]);
                GUI.DrawTexture(position, texture2);
            }
        }
    }

    public void SetButtonContent(ECardListButton button, GUIContent content)
    {
        switch (button)
        {
            case ECardListButton.BUTTON_LEFT:
                this.buttonLeftContent = content;
                break;

            case ECardListButton.BUTTON_RIGHT:
                this.buttonRightContent = content;
                break;
        }
    }

    public void SetButtonEnabled(ECardListButton button, bool enabled)
    {
        switch (button)
        {
            case ECardListButton.BUTTON_LEFT:
                this.leftButtonEnabled = enabled;
                break;

            case ECardListButton.BUTTON_RIGHT:
                this.rightButtonEnabled = enabled;
                break;
        }
    }

    public void SetButtonHighlighted(ECardListButton button, bool highlighted)
    {
        switch (button)
        {
            case ECardListButton.BUTTON_LEFT:
                this.leftButtonHighlighted = highlighted;
                break;

            case ECardListButton.BUTTON_RIGHT:
                this.rightButtonHighlighted = highlighted;
                break;
        }
    }

    public void SetItemList(List<Item> cards)
    {
        this.cards = cards;
    }

    public void SetOffX(float offX)
    {
        this.offX = offX;
    }

    public void SetOpacity(float opacity)
    {
        this.opacity = opacity;
    }

    private void Start()
    {
        this.lobbySkin = (GUISkin) Resources.Load("_GUISkins/Lobby");
        this.cardListPopupSkin = (GUISkin) Resources.Load("_GUISkins/CardListPopup");
        this.cardListPopupGradientSkin = (GUISkin) Resources.Load("_GUISkins/CardListPopupGradient");
        this.cardListPopupBigLabelSkin = (GUISkin) Resources.Load("_GUISkins/CardListPopupBigLabel");
        this.cardListPopupLeftButtonSkin = (GUISkin) Resources.Load("_GUISkins/CardListPopupLeftButton");
    }

    private void Update()
    {
        Vector3 point = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        bool flag = this.innerRect.Contains(point);
        bool flag2 = false;
        int num = 0;
        foreach (Item card in this.cards)
        {
            Rect rect = new Rect(0f, num * this.fieldHeight, this.innerRect.width - this.scrollBarSize, this.fieldHeight);
            if (flag && rect.Contains(point - new Vector3(this.innerRect.x - this.scrollPos.x, this.innerRect.y - this.scrollPos.y)))
            {
                flag2 = true;
                this.callback.ItemHovered(this, card);
                break;
            }
            num++;
        }
        if (!flag2)
        {
            this.callback.ItemHovered(this, null);
        }
    }
}
}
