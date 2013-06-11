using Gui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EndGameScreen : MonoBehaviour, ICommListener
{
    private float avatarXFactor;
    private GUISkin blackLabelBGSkin;
    private bool done;
    private EMEndGame endGameStatistics;
    private float gameOverAlpha;
    private float gameOverOverlayAlpha;
    private GameType gameType;
    private float headerYFactor;
    private bool inited;
    private bool isWinner;
    private GUISkin labelSkin;
    private List<Texture> leftAvatarTextures;
    private string leftName;
    private TileColor playerColor;
    private float ratingAlpha;
    private float ratingT;
    private RatingUpdateMessage ratingUpdate;
    private List<Texture> rightAvatarTextures;
    private string rightName;
    private bool showingStats;
    [SerializeField]
    private Color statsColor = Color.white;
    [SerializeField]
    private float statsFontSize = 0.027f;
    private float statsGoldAlpha;
    [SerializeField]
    private Color statsGoldColor = Color.white;
    [SerializeField]
    private float statsGoldFontSize = 0.027f;
    [SerializeField]
    private float statsGoldYPos = 0.7f;
    [SerializeField]
    private float statsLineHeight = 1.1f;
    [SerializeField]
    private Vector2 statsPosition = new Vector2(0.13f, 0.5f);
    private GUISkin statsSkin;
    [SerializeField]
    private float statsWidth = 0.2f;
    private Texture2D titleTex;
    private float tValue;
    private float victorySlamAlpha;

    [DebuggerHidden]
    private IEnumerator FadeInRatingUpdates()
    {
        return new <FadeInRatingUpdates>c__Iterator10 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator FadeInStats()
    {
        return new <FadeInStats>c__IteratorE { <>f__this = this };
    }

    private void GoToLobby()
    {
        App.Communicator.sendBattleRequest(new LeaveGameMessage());
        App.Communicator.joinLobby(true);
    }

    public static void GUIDrawAvatar(List<Texture> textures, Rect rect, bool headingLeft)
    {
        GUIDrawAvatar(UnityGui2D.getInstance(), textures, rect, headingLeft);
    }

    public static void GUIDrawAvatar(IGui gui, List<Texture> textures, Rect rect, bool headingLeft)
    {
        Rect texCoords = new Rect(!headingLeft ? ((float) 1) : ((float) 0), 0f, !headingLeft ? ((float) (-1)) : ((float) 1), 1f);
        foreach (Texture texture in textures)
        {
            Rect dst = new Rect(rect);
            if (texture.width != Avatar.DefaultWidth)
            {
                dst.width = (int) (dst.width * (((float) texture.width) / ((float) Avatar.DefaultWidth)));
                if (!headingLeft)
                {
                    dst.x = rect.x - (dst.width - rect.width);
                }
            }
            gui.DrawTextureWithTexCoords(dst, texture, texCoords);
        }
    }

    public void handleMessage(Message msg)
    {
        if (msg is RatingUpdateMessage)
        {
            this.ratingUpdate = (RatingUpdateMessage) msg;
            base.StopCoroutine("FadeInRatingUpdates");
            base.StartCoroutine("FadeInRatingUpdates");
        }
    }

    public void init(GameType gameType, TileColor playerColor, EMEndGame endGameStatistics, AvatarInfo leftAvatar, AvatarInfo rightAvatar, string leftName, string rightName)
    {
        this.inited = true;
        this.gameType = gameType;
        this.playerColor = playerColor;
        this.endGameStatistics = endGameStatistics;
        this.leftName = leftName;
        this.rightName = rightName;
        this.leftAvatarTextures = Avatar.getTexturesFor(leftAvatar, 0);
        this.rightAvatarTextures = Avatar.getTexturesFor(rightAvatar, 0);
        this.isWinner = playerColor == endGameStatistics.winner;
        if (this.isWinner)
        {
            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_victory");
        }
        else
        {
            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_defeat");
        }
        this.titleTex = !this.isWinner ? ResourceManager.LoadTexture("BattleMode/GUI/title_defeat") : ResourceManager.LoadTexture("BattleMode/GUI/title_victory");
        this.blackLabelBGSkin = (GUISkin) Resources.Load("_GUISkins/BlackLabelBG");
        this.statsSkin = (GUISkin) Resources.Load("_GUISkins/StatsSkin");
        this.labelSkin = ScriptableObject.CreateInstance<GUISkin>();
        this.labelSkin.label.wordWrap = false;
        this.labelSkin.label.font = (Font) Resources.Load("Fonts/arial", typeof(Font));
        this.labelSkin.label.fontSize = 0x10;
        this.labelSkin.label.normal.background = ResourceManager.LoadTexture("BattleMode/blackDot");
        this.labelSkin.label.normal.textColor = new Color(1f, 1f, 1f, 1f);
        this.labelSkin.label.fontStyle = FontStyle.Bold;
        base.StartCoroutine(this.FadeInStats());
        base.StartCoroutine(this.MoveInAvatars());
    }

    public bool isDone()
    {
        return this.done;
    }

    public bool isInited()
    {
        return this.inited;
    }

    [DebuggerHidden]
    private IEnumerator MoveInAvatars()
    {
        return new <MoveInAvatars>c__IteratorF { <>f__this = this };
    }

    private void OnDestroy()
    {
        App.Communicator.removeListener(this);
    }

    public void OnGUI()
    {
        if (this.inited)
        {
            GUI.depth = 7;
            GUISkin skin = GUI.skin;
            Color color = GUI.color;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.gameOverOverlayAlpha * 0.8f);
            GUI.skin = this.blackLabelBGSkin;
            GUI.Label(new Rect(-50f, -50f, (float) (Screen.width + 100), (float) (Screen.height + 100)), string.Empty);
            GUI.skin = skin;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.gameOverAlpha);
            float num = (0.75f + ((1f - this.headerYFactor) * 0.25f)) + ((1f - this.gameOverAlpha) * 1.5f);
            float num2 = this.titleTex.height * num;
            float num3 = Screen.height * 0.05f;
            float top = ((num3 + (Screen.height / 2)) - (num2 * 0.5f)) - (this.headerYFactor * ((Screen.height / 2) - (num2 * 0.3f)));
            GUI.DrawTexture(new Rect((Screen.width / 2) - ((this.titleTex.width / 2) * num), top, this.titleTex.width * num, this.titleTex.height * num), this.titleTex);
            if (this.victorySlamAlpha > 0f)
            {
                float victorySlamAlpha = this.victorySlamAlpha;
                num *= 1f + (victorySlamAlpha * 0.75f);
                num2 = this.titleTex.height * num;
                top = ((num3 + (Screen.height / 2)) - (num2 * 0.5f)) - (this.headerYFactor * ((Screen.height / 2) - (num2 * 0.3f)));
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1f - Mathf.Pow(victorySlamAlpha, 0.7f));
                GUI.DrawTexture(new Rect((Screen.width / 2) - ((this.titleTex.width / 2) * num), top, this.titleTex.width * num, this.titleTex.height * num), this.titleTex);
            }
            GUI.skin = this.labelSkin;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.statsGoldAlpha);
            GUI.skin = this.statsSkin;
            if (GUI.Button(new Rect((Screen.width / 2) - (Screen.height * 0.15f), Screen.height * 0.9f, Screen.height * 0.3f, Screen.height * 0.05f), "Leave match"))
            {
                App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                this.done = true;
                this.GoToLobby();
            }
            GUI.skin.label.fontSize = (int) (Screen.height * this.statsFontSize);
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            float height = (Screen.height * this.statsFontSize) * this.statsLineHeight;
            float num7 = (Screen.height * this.statsGoldFontSize) * this.statsLineHeight;
            float width = Screen.width * this.statsWidth;
            float left = Screen.width * this.statsPosition.x;
            float num10 = Screen.height * this.statsPosition.y;
            TileColor color3 = TileColorUtil.otherColor(this.playerColor);
            GameStatistics statistics = this.endGameStatistics.getGameStatistics(this.playerColor);
            GameStatistics statistics2 = this.endGameStatistics.getGameStatistics(color3);
            string[] strArray = new string[] { "Damage (idols)", "Damage (units)", "Units played", "Spells cast", "Enchantments", "Scrolls drawn", "Highest damage" };
            int[] numArray = new int[] { statistics.idolDamage, statistics2.idolDamage, statistics.unitDamage, statistics2.unitDamage, statistics.unitsPlayed, statistics2.unitsPlayed, statistics.spellsPlayed, statistics2.spellsPlayed, statistics.enchantmentsPlayed, statistics2.enchantmentsPlayed, statistics.scrollsDrawn, statistics2.scrollsDrawn, statistics.mostDamageUnit, statistics2.mostDamageUnit };
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.statsGoldAlpha);
            Color textColor = GUI.skin.label.normal.textColor;
            GUI.skin.label.normal.textColor = this.statsColor;
            TextAnchor alignment = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(left + (width * 0.4f), num10 - height, width * 0.27f, height), this.leftName);
            GUI.Label(new Rect(left + (width * 0.73f), num10 - height, width * 0.27f, height), this.rightName);
            GUI.skin.label.alignment = alignment;
            int num11 = 0;
            for (int i = 0; i < strArray.Length; i++)
            {
                Color color5 = GUI.color;
                if ((num11 % 2) == 0)
                {
                    GUI.color = new Color(color5.r, color5.g, color5.b, color5.a * 0.4f);
                }
                else
                {
                    GUI.color = new Color(color5.r, color5.g, color5.b, color5.a * 0.2f);
                }
                GUI.Box(new Rect(left - (width * 0.05f), num10 + (height * i), width * 1.1f, height * 1.1f), string.Empty);
                GUI.color = color5;
                GUI.Label(new Rect(left, num10 + (height * i), width, height), strArray[i]);
                num11++;
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            for (int j = 0; j < 2; j++)
            {
                for (int n = j; n < numArray.Length; n += 2)
                {
                    GUI.Label(new Rect((left + (width * 0.4f)) + ((j * width) * 0.33f), num10 + (height * (n / 2)), width * 0.25f, height), numArray[n].ToString());
                }
            }
            GUI.skin.label.alignment = alignment;
            if (this.ratingUpdate != null)
            {
                int[] numArray2;
                num10 += height * strArray.Length;
                string[] strArray2 = new string[] { "Rating change", "New rating" };
                if (this.playerColor == TileColor.white)
                {
                    numArray2 = new int[] { this.ratingUpdate.whiteChange, this.ratingUpdate.blackChange, this.ratingUpdate.whiteNewRating, this.ratingUpdate.blackNewRating };
                }
                else if (this.playerColor == TileColor.black)
                {
                    numArray2 = new int[] { this.ratingUpdate.blackChange, this.ratingUpdate.whiteChange, this.ratingUpdate.blackNewRating, this.ratingUpdate.whiteNewRating };
                }
                else
                {
                    numArray2 = new int[4];
                }
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.ratingAlpha);
                GUI.skin.label.normal.textColor = this.statsColor;
                for (int num15 = 0; num15 < strArray2.Length; num15++)
                {
                    Color color6 = GUI.color;
                    if ((num11 % 2) == 0)
                    {
                        GUI.color = new Color(color6.r, color6.g, color6.b, color6.a * 0.4f);
                    }
                    else
                    {
                        GUI.color = new Color(color6.r, color6.g, color6.b, color6.a * 0.2f);
                    }
                    GUI.Box(new Rect(left - (width * 0.05f), num10 + (height * num15), width * 1.1f, height * 1.1f), string.Empty);
                    GUI.color = color6;
                    GUI.Label(new Rect(left, num10 + (height * num15), width, height), strArray2[num15]);
                    num11++;
                }
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                for (int num16 = 0; num16 < 2; num16++)
                {
                    for (int num17 = num16; num17 < numArray2.Length; num17 += 2)
                    {
                        GUI.Label(new Rect((left + (width * 0.4f)) + ((num16 * width) * 0.33f), num10 + (height * (num17 / 2)), width * 0.25f, height), numArray2[num17].ToString());
                    }
                }
                GUI.skin.label.alignment = alignment;
            }
            TowerLevels challengeInfo = this.endGameStatistics.challengeInfo;
            List<string> list = new List<string>();
            string item = !this.isWinner ? "Match" : "Victory";
            list.Add(item);
            list.Add("Completion");
            list.Add("Idols");
            if ((challengeInfo != null) && challengeInfo.isCompleted)
            {
                list.Add("Trial");
            }
            list.Add("Total");
            GameRewardStatistics statistics3 = this.endGameStatistics.getGameRewardStatistics(this.playerColor);
            TextAnchor anchor2 = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.UpperCenter;
            GUI.skin.label.normal.textColor = new Color(0.6f, 0.5f, 0.3f);
            if (this.gameType.isMultiplayerChallenge())
            {
                GUI.Label(new Rect((Screen.width / 2) - (Screen.height * 0.4f), Screen.height * 0.96f, Screen.height * 0.8f, Screen.height * 0.03f), "* Gold is only awarded for the first five challenge matches of the day");
            }
            GUI.skin.label.normal.textColor = this.statsGoldColor;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.statsGoldAlpha);
            GUI.skin.label.fontSize = (int) ((Screen.height * this.statsGoldFontSize) * 1.5f);
            GUI.Label(new Rect(Screen.width * 0.38f, (Screen.height * this.statsGoldYPos) - ((list.Count != 5) ? 0f : (num7 * 0.5f)), Screen.width * 0.24f, num7 * 1.3f), !this.gameType.isMultiplayerChallenge() ? "Rewards" : " Rewards*");
            GUI.skin.label.alignment = anchor2;
            GUI.skin.label.fontSize = (int) (Screen.height * this.statsGoldFontSize);
            float num18 = (Screen.height * this.statsGoldYPos) + (num7 * 1.5f);
            for (int k = 0; k < list.Count; k++)
            {
                GUI.Label(new Rect(Screen.width * 0.395f, (num18 + (num7 * k)) - ((list.Count != 5) ? 0f : (num7 * 0.5f)), Screen.width * 0.24f, num7), list[k]);
            }
            List<string> list2 = new List<string> {
                statistics3.matchReward.ToString(),
                statistics3.matchCompletionReward.ToString(),
                statistics3.idolsDestroyedReward.ToString()
            };
            if ((challengeInfo != null) && challengeInfo.isCompleted)
            {
                string str2 = (challengeInfo.goldReward <= 0) ? string.Empty : challengeInfo.goldReward.ToString();
                list2.Add(str2);
            }
            list2.Add((statistics3.totalReward + (((challengeInfo == null) || !challengeInfo.isCompleted) ? 0 : challengeInfo.goldReward)).ToString());
            for (int m = 0; m < list2.Count; m++)
            {
                GUI.Label(new Rect(Screen.width * 0.593f, (num18 + (num7 * m)) - ((list2.Count != 5) ? 0f : (num7 * 0.5f)), Screen.width * 0.24f, num7), list2[m]);
            }
            GUI.skin.label.normal.textColor = textColor;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.gameOverAlpha);
            float num22 = Screen.height * 0.8f;
            float num23 = (num22 * 567f) / 991f;
            Rect rect = new Rect((-1f + this.avatarXFactor) * Screen.width, Screen.height * 0.17f, num23, num22);
            Rect rect2 = new Rect((Screen.width - rect.x) - rect.width, rect.y, rect.width, rect.height);
            GUIDrawAvatar(this.leftAvatarTextures, rect, false);
            GUIDrawAvatar(this.rightAvatarTextures, rect2, true);
            GUI.color = color;
        }
    }

    public virtual void onReconnect()
    {
    }

    private void Start()
    {
        App.Communicator.addListener(this);
    }

    [CompilerGenerated]
    private sealed class <FadeInRatingUpdates>c__Iterator10 : IEnumerator<object>, IEnumerator, IDisposable
    {
        internal object $current;
        internal int $PC;
        internal EndGameScreen <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (!this.<>f__this.showingStats)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_010F;
                    }
                    this.<>f__this.ratingT = 0f;
                    break;

                case 2:
                    break;

                default:
                    goto Label_010D;
            }
            while (this.<>f__this.ratingT < 1f)
            {
                this.<>f__this.ratingT += Time.deltaTime / 1.7f;
                if (this.<>f__this.ratingT > 1f)
                {
                    this.<>f__this.ratingT = 1f;
                }
                this.<>f__this.ratingAlpha = (this.<>f__this.ratingT * this.<>f__this.ratingT) * (3f - (2f * this.<>f__this.ratingT));
                this.$current = null;
                this.$PC = 2;
                goto Label_010F;
            }
            this.$PC = -1;
        Label_010D:
            return false;
        Label_010F:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <FadeInStats>c__IteratorE : IEnumerator<object>, IEnumerator, IDisposable
    {
        internal object $current;
        internal int $PC;
        internal EndGameScreen <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.tValue = 0f;
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_019E;

                case 3:
                    this.<>f__this.tValue = 0f;
                    goto Label_0273;

                case 4:
                    goto Label_0273;

                case 5:
                    this.<>f__this.showingStats = true;
                    this.<>f__this.tValue = 0f;
                    goto Label_034E;

                case 6:
                    goto Label_034E;

                default:
                    goto Label_036A;
            }
            if (this.<>f__this.tValue < 1f)
            {
                this.<>f__this.tValue += Time.deltaTime * 2f;
                if (this.<>f__this.tValue > 1f)
                {
                    this.<>f__this.tValue = 1f;
                }
                this.<>f__this.gameOverOverlayAlpha = (this.<>f__this.tValue * this.<>f__this.tValue) * (3f - (2f * this.<>f__this.tValue));
                this.<>f__this.gameOverAlpha = Mathf.Pow(this.<>f__this.tValue, 2f);
                this.$current = null;
                this.$PC = 1;
                goto Label_036C;
            }
            if (!this.<>f__this.isWinner)
            {
                goto Label_01B3;
            }
            this.<>f__this.tValue = 0f;
        Label_019E:
            while (this.<>f__this.tValue < 1f)
            {
                this.<>f__this.tValue += Time.deltaTime * 2f;
                if (this.<>f__this.tValue > 1f)
                {
                    this.<>f__this.tValue = 1f;
                }
                this.<>f__this.victorySlamAlpha = this.<>f__this.tValue;
                this.$current = null;
                this.$PC = 2;
                goto Label_036C;
            }
        Label_01B3:
            this.$current = new WaitForSeconds(0.3f);
            this.$PC = 3;
            goto Label_036C;
        Label_0273:
            if (this.<>f__this.tValue < 1f)
            {
                this.<>f__this.tValue += Time.deltaTime * 3.5f;
                if (this.<>f__this.tValue > 1f)
                {
                    this.<>f__this.tValue = 1f;
                }
                this.<>f__this.headerYFactor = (this.<>f__this.tValue * this.<>f__this.tValue) * (3f - (2f * this.<>f__this.tValue));
                this.$current = null;
                this.$PC = 4;
            }
            else
            {
                this.$current = new WaitForSeconds(0f);
                this.$PC = 5;
            }
            goto Label_036C;
        Label_034E:
            while (this.<>f__this.tValue < 1f)
            {
                this.<>f__this.tValue += Time.deltaTime;
                if (this.<>f__this.tValue > 1f)
                {
                    this.<>f__this.tValue = 1f;
                }
                this.<>f__this.statsGoldAlpha = (this.<>f__this.tValue * this.<>f__this.tValue) * (3f - (2f * this.<>f__this.tValue));
                this.$current = null;
                this.$PC = 6;
                goto Label_036C;
            }
            this.$PC = -1;
        Label_036A:
            return false;
        Label_036C:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <MoveInAvatars>c__IteratorF : IEnumerator<object>, IEnumerator, IDisposable
    {
        internal object $current;
        internal int $PC;
        internal EndGameScreen <>f__this;
        internal float <t>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(0.75f);
                    this.$PC = 1;
                    goto Label_00D5;

                case 1:
                    this.<t>__0 = 0f;
                    break;

                case 2:
                    break;

                default:
                    goto Label_00D3;
            }
            if (this.<t>__0 < 1f)
            {
                this.<t>__0 += Time.deltaTime;
                if (this.<t>__0 > 1f)
                {
                    this.<t>__0 = 1f;
                }
                this.<>f__this.avatarXFactor = (this.<t>__0 * this.<t>__0) * (3f - (2f * this.<t>__0));
                this.$current = null;
                this.$PC = 2;
                goto Label_00D5;
            }
            this.$PC = -1;
        Label_00D3:
            return false;
        Label_00D5:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

