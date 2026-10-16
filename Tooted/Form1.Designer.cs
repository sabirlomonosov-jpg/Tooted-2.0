namespace Tooted
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtNimetus = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numKogus = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numHind = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbKategooria = new System.Windows.Forms.ComboBox();
            this.picPilt = new System.Windows.Forms.PictureBox();
            this.lisaKategooria_btn = new System.Windows.Forms.Button();
            this.btnKustutaKategooria = new System.Windows.Forms.Button();
            this.btnOtsiFail = new System.Windows.Forms.Button();
            this.btnLisa = new System.Windows.Forms.Button();
            this.btnUuenda = new System.Windows.Forms.Button();
            this.btnKustuta = new System.Windows.Forms.Button();
            this.dataGridTooted = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.numKogus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHind)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPilt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTooted)).BeginInit();
            this.SuspendLayout();
            // 
            // txtNimetus
            // 
            this.txtNimetus.Location = new System.Drawing.Point(179, 33);
            this.txtNimetus.Name = "txtNimetus";
            this.txtNimetus.Size = new System.Drawing.Size(121, 20);
            this.txtNimetus.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label1.Location = new System.Drawing.Point(25, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 36);
            this.label1.TabIndex = 1;
            this.label1.Text = "Toode nimetus";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label2.Location = new System.Drawing.Point(60, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Kogus";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // numKogus
            // 
            this.numKogus.Location = new System.Drawing.Point(179, 82);
            this.numKogus.Name = "numKogus";
            this.numKogus.Size = new System.Drawing.Size(121, 20);
            this.numKogus.TabIndex = 3;
            this.numKogus.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.numKogus.Minimum = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label3.Location = new System.Drawing.Point(118, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Hind";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numHind
            // 
            this.numHind.Location = new System.Drawing.Point(179, 131);
            this.numHind.Name = "numHind";
            this.numHind.Size = new System.Drawing.Size(121, 20);
            this.numHind.TabIndex = 5;
            this.numHind.DecimalPlaces = 2;
            this.numHind.Maximum = 99999999.99M;
            this.numHind.Increment = 0.10M;
            this.numHind.Minimum = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label4.Location = new System.Drawing.Point(70, 180);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "Kategooria";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbKategooria
            // 
            this.cmbKategooria.FormattingEnabled = true;
            this.cmbKategooria.Location = new System.Drawing.Point(179, 180);
            this.cmbKategooria.Name = "cmbKategooria";
            this.cmbKategooria.Size = new System.Drawing.Size(121, 21);
            this.cmbKategooria.TabIndex = 7;
            // 
            // picPilt
            // 
            this.picPilt.Location = new System.Drawing.Point(359, 33);
            this.picPilt.Name = "picPilt";
            this.picPilt.Size = new System.Drawing.Size(389, 168);
            this.picPilt.TabIndex = 8;
            this.picPilt.TabStop = false;
            this.picPilt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            // 
            // lisaKategooria_btn
            // 
            this.lisaKategooria_btn.Location = new System.Drawing.Point(12, 219);
            this.lisaKategooria_btn.Name = "lisaKategooria_btn";
            this.lisaKategooria_btn.Size = new System.Drawing.Size(123, 44);
            this.lisaKategooria_btn.TabIndex = 9;
            this.lisaKategooria_btn.Text = "Lisa kategooria";
            this.lisaKategooria_btn.UseVisualStyleBackColor = true;
            this.lisaKategooria_btn.Click += new System.EventHandler(this.lisaKategooria_btn_Click);
            // 
            // btnKustutaKategooria
            // 
            this.btnKustutaKategooria.Location = new System.Drawing.Point(141, 219);
            this.btnKustutaKategooria.Name = "btnKustutaKategooria";
            this.btnKustutaKategooria.Size = new System.Drawing.Size(131, 44);
            this.btnKustutaKategooria.TabIndex = 10;
            this.btnKustutaKategooria.Text = "Kustuta kategooria";
            this.btnKustutaKategooria.UseVisualStyleBackColor = true;
            // 
            // btnOtsiFail
            // 
            this.btnOtsiFail.Location = new System.Drawing.Point(278, 219);
            this.btnOtsiFail.Name = "btnOtsiFail";
            this.btnOtsiFail.Size = new System.Drawing.Size(123, 44);
            this.btnOtsiFail.TabIndex = 11;
            this.btnOtsiFail.Text = "Otsi faili";
            this.btnOtsiFail.UseVisualStyleBackColor = true;
            this.btnOtsiFail.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnLisa
            // 
            this.btnLisa.Location = new System.Drawing.Point(407, 219);
            this.btnLisa.Name = "btnLisa";
            this.btnLisa.Size = new System.Drawing.Size(123, 44);
            this.btnLisa.TabIndex = 12;
            this.btnLisa.Text = "Lisa";
            this.btnLisa.UseVisualStyleBackColor = true;
            // 
            // btnUuenda
            // 
            this.btnUuenda.Location = new System.Drawing.Point(536, 219);
            this.btnUuenda.Name = "btnUuenda";
            this.btnUuenda.Size = new System.Drawing.Size(123, 44);
            this.btnUuenda.TabIndex = 13;
            this.btnUuenda.Text = "Uuenda";
            this.btnUuenda.UseVisualStyleBackColor = true;
            // 
            // btnKustuta
            // 
            this.btnKustuta.Location = new System.Drawing.Point(665, 219);
            this.btnKustuta.Name = "btnKustuta";
            this.btnKustuta.Size = new System.Drawing.Size(123, 44);
            this.btnKustuta.TabIndex = 14;
            this.btnKustuta.Text = "Kustuta";
            this.btnKustuta.UseVisualStyleBackColor = true;
            // 
            // dataGridTooted
            // 
            this.dataGridTooted.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridTooted.Location = new System.Drawing.Point(12, 269);
            this.dataGridTooted.Name = "dataGridTooted";
            this.dataGridTooted.Size = new System.Drawing.Size(776, 169);
            this.dataGridTooted.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataGridTooted);
            this.Controls.Add(this.btnKustuta);
            this.Controls.Add(this.btnUuenda);
            this.Controls.Add(this.btnLisa);
            this.Controls.Add(this.btnOtsiFail);
            this.Controls.Add(this.btnKustutaKategooria);
            this.Controls.Add(this.lisaKategooria_btn);
            this.Controls.Add(this.picPilt);
            this.Controls.Add(this.cmbKategooria);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numHind);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numKogus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNimetus);
            this.Name = "Form1";
            this.Text = "Toodete haldus";
            ((System.ComponentModel.ISupportInitialize)(this.numKogus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHind)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPilt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTooted)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtNimetus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numKogus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numHind;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbKategooria;
        private System.Windows.Forms.PictureBox picPilt;
        private System.Windows.Forms.Button lisaKategooria_btn;
        private System.Windows.Forms.Button btnKustutaKategooria;
        private System.Windows.Forms.Button btnOtsiFail;
        private System.Windows.Forms.Button btnLisa;
        private System.Windows.Forms.Button btnUuenda;
        private System.Windows.Forms.Button btnKustuta;
        private System.Windows.Forms.DataGridView dataGridTooted;
    }
}

