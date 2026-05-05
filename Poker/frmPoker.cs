using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poker
{
    public partial class frmPoker : Form
    {
        #region 欄位
        /// <summary>
        /// 用來存放牌桌上五張牌的 PictureBox 陣列
        /// </summary>
        PictureBox[] pic = new PictureBox[5];

        /// <summary>
        /// 所有的牌的編號，從 0 到 51
        /// </summary>
        int[] allPoker = new int[52];

        /// <summary>
        /// 記錄玩家手牌的編號
        /// </summary>
        int[] playerPoker = new int[5];

        // --- 下注相關欄位 ---
        int totalMoney = 1000000; // 總資金
        int betMoney = 0;         // 本局押注金額
        #endregion

        public frmPoker()
        {
            InitializeComponent();
            InitializePoker();
            
            // 初始化 UI 數值
            txtTotalMoney.Text = totalMoney.ToString();
            txtBet.Text = "500";
            lblResult.Text = "請輸入金額並點擊押注";

            // 初始按鈕狀態控制
            btnDealCard.Enabled = false;
            btnChangeCard.Enabled = false;
            btnCheck.Enabled = false;
        }

        #region 自定義方法
        private void InitializePoker()
        {
            for (int i = 0; i < pic.Length; i++)
            {
                pic[i] = new PictureBox();
                pic[i].Image = GetImage("back");
                pic[i].Name = "pic" + i;
                pic[i].SizeMode = PictureBoxSizeMode.AutoSize;
                pic[i].Top = 30;
                pic[i].Left = 10 + ((100 + 10) * i); // 假設寬度約100
                pic[i].Enabled = false;
                pic[i].Tag = "back";
                pic[i].Visible = true;

                this.grpPoker.Controls.Add(pic[i]);
                pic[i].Click += Pic_Click;
            }
        }

        private void ShowCards()
        {
            for (int i = 0; i < playerPoker.Length; i++)
            {
                pic[i].Image = this.GetImage($"pic{playerPoker[i] + 1}");
            }
        }

        private Image GetImage(string name)
        {
            return Properties.Resources.ResourceManager.GetObject(name) as Image;
        }

        private Image GetImage(int num)
        {
            return GetImage($"pic{num}");
        }

        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < allPoker.Length; i++) allPoker[i] = i; // 確保重置牌堆

            for (int i = 0; i < 1000; i++)
            {
                int r = rand.Next(allPoker.Length);
                int temp = allPoker[r];
                allPoker[r] = allPoker[0];
                allPoker[0] = temp;
            }
        }
        #endregion

        #region 事件處理程序

        /// <summary>
        /// 下注按鈕：扣除資金並開啟發牌功能
        /// </summary>

        private void Pic_Click(object sender, EventArgs e)
        {
            PictureBox picBox = sender as PictureBox;
            int index = int.Parse(picBox.Name.Replace("pic", ""));
            int cardNum = playerPoker[index] + 1;

            if (picBox.Tag.ToString() == "back")
            {
                picBox.Tag = "front";
                picBox.Image = GetImage(cardNum);
            }
            else
            {
                picBox.Tag = "back";
                picBox.Image = GetImage("back");
            }
        }

        private async void btnDealCard_Click(object sender, EventArgs e)
        {
            lblResult.Text = "洗牌中...";
            
            // 重置牌面
            for (int i = 0; i < pic.Length; i++) pic[i].Image = GetImage("back");

            Shuffle();
            await Task.Delay(500);

            // 發牌
            for (int i = 0; i < playerPoker.Length; i++) playerPoker[i] = allPoker[i];

            ShowCards();

            for (int i = 0; i < pic.Length; i++)
            {
                pic[i].Enabled = true;
                pic[i].Tag = "front";
            }

            btnDealCard.Enabled = false;
            btnChangeCard.Enabled = true;
            lblResult.Text = "點選想換掉的牌使其背面朝上，然後按換牌";
        }

        private void btnChangeCard_Click(object sender, EventArgs e)
        {
            int startIndex = 5; 
            for(int i = 0; i < playerPoker.Length; i++)
            {
                if (pic[i].Tag.ToString() == "back")
                {
                    playerPoker[i] = allPoker[startIndex++];
                    pic[i].Image = GetImage(playerPoker[i] + 1);
                    pic[i].Tag = "front";
                }
                pic[i].Enabled = false;
            }

            btnChangeCard.Enabled = false;
            btnCheck.Enabled = true;
            lblResult.Text = "換牌完成，請點擊判斷牌型";
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            string[] colorList = { "梅花", "方塊", "愛心", "黑桃" };
            string[] pointList = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

            int[] pokerColor = new int[5];
            int[] pokerPoint = new int[5];

            for (int i = 0; i < playerPoker.Length; i++)
            {
                pokerColor[i] = playerPoker[i] % 4;
                pokerPoint[i] = playerPoker[i] / 4;
            }

            int[] colorCount = new int[4];
            int[] pointCount = new int[13];
            for (int i = 0; i < 5; i++)
            {
                colorCount[pokerColor[i]]++;
                pointCount[pokerPoint[i]]++;
            }

            // 用於判斷的邏輯
            Array.Sort(pointCount);
            Array.Reverse(pointCount);
            Array.Sort(colorCount);
            Array.Reverse(colorCount);

            bool isFlush = (colorCount[0] == 5);
            bool isRoyal = new int[] { 0, 9, 10, 11, 12 }.All(p => pokerPoint.Contains(p));
            bool isStraight = (pokerPoint.Max() - pokerPoint.Min() == 4 && pointCount[0] == 1) || isRoyal;
            
            string handName = "";
            int odds = 0;

            // 根據 PDF 要求設定賠率
            if (isFlush && isRoyal) { handName = "皇家同花順"; odds = 250; }
            else if (isFlush && isStraight) { handName = "同花順"; odds = 50; }
            else if (pointCount[0] == 4) { handName = "四條"; odds = 25; }
            else if (pointCount[0] == 3 && pointCount[1] == 2) { handName = "葫蘆"; odds = 9; }
            else if (isFlush) { handName = "同花"; odds = 6; }
            else if (isStraight) { handName = "順子"; odds = 4; }
            else if (pointCount[0] == 3) { handName = "三條"; odds = 3; }
            else if (pointCount[0] == 2 && pointCount[1] == 2) { handName = "兩對"; odds = 2; }
            else if (pointCount[0] == 2) { handName = "一對"; odds = 1; }
            else { handName = "雜牌"; odds = 0; }

            // 計算獎金並更新資金
            int winMoney = betMoney * odds;
            totalMoney += winMoney;
            
            lblResult.Text = $"{handName}！(賠率:{odds}) 獲得獎金: {winMoney}";
            txtTotalMoney.Text = totalMoney.ToString();

            // 遊戲循環重置
            btnCheck.Enabled = false;
            btnBet.Enabled = true;
            txtBet.Enabled = true;
            btnDealCard.Enabled = false;
        }

        private void frmPoker_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 此處保留原有的 Q/W/E/R 測試碼，用於快速測試各種牌型賠率
            if (btnDealCard.Enabled == false && btnChangeCard.Enabled == true)
            {
                switch(e.KeyChar)
                {
                    case 'q': // 皇家同花順
                        playerPoker[0] = 51; playerPoker[1] = 47; playerPoker[2] = 43; playerPoker[3] = 39; playerPoker[4] = 3;
                        break;
                    case 'w': // 同花順
                        playerPoker[0] = 37; playerPoker[1] = 33; playerPoker[2] = 29; playerPoker[3] = 25; playerPoker[4] = 21;
                        break;
                }
                ShowCards();
            }
        }
        #endregion
        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // 留空即可
        }

        private void btnBet_Click_1(object sender, EventArgs e)
        {
            // 取得押注金額並確認格式正確
            if (int.TryParse(txtBet.Text, out betMoney) && betMoney > 0)
            {
                // 檢查是否破產（餘額小於押注金額）
                if (totalMoney < betMoney)
                {
                    DialogResult dr = MessageBox.Show("資金不足！是否重回 1,000,000 元？", "破產重生", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        totalMoney = 1000000; // 重設資金
                        txtTotalMoney.Text = totalMoney.ToString();
                        return; // 重新整理資金後讓玩家再次點擊押注
                    }
                    else
                    {
                        return; // 玩家不重設，則不執行後續發牌
                    }
                }

                // 正常扣款與啟動流程
                totalMoney -= betMoney;
                txtTotalMoney.Text = totalMoney.ToString();

                btnBet.Enabled = false;    // 禁用押注鈕
                btnDealCard.Enabled = true; // 啟用發牌鈕
                txtBet.Enabled = false;    // 鎖定押注輸入框
                lblResult.Text = "押注成功，請開始發牌";
            }
            else
            {
                MessageBox.Show("請輸入正確的押注金額。");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}