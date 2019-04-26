namespace Evolution.ModelBuilder {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtClassNameSingular = new System.Windows.Forms.TextBox();
            this.txtClassNamePlural = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLinqVarName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtNameSpaceRoot = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkCreateServiceClass = new System.Windows.Forms.CheckBox();
            this.txtServiceName = new System.Windows.Forms.TextBox();
            this.chkCreateModelClass = new System.Windows.Forms.CheckBox();
            this.chkCreateViewModelClass = new System.Windows.Forms.CheckBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Class Name (Singular)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Class Name (Plural)";
            // 
            // txtClassNameSingular
            // 
            this.txtClassNameSingular.Location = new System.Drawing.Point(214, 15);
            this.txtClassNameSingular.Name = "txtClassNameSingular";
            this.txtClassNameSingular.Size = new System.Drawing.Size(304, 26);
            this.txtClassNameSingular.TabIndex = 1;
            // 
            // txtClassNamePlural
            // 
            this.txtClassNamePlural.Location = new System.Drawing.Point(214, 50);
            this.txtClassNamePlural.Name = "txtClassNamePlural";
            this.txtClassNamePlural.Size = new System.Drawing.Size(304, 26);
            this.txtClassNamePlural.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(147, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Linq Variable Name";
            // 
            // txtLinqVarName
            // 
            this.txtLinqVarName.Location = new System.Drawing.Point(214, 86);
            this.txtLinqVarName.Name = "txtLinqVarName";
            this.txtLinqVarName.Size = new System.Drawing.Size(304, 26);
            this.txtLinqVarName.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Namespace Root";
            // 
            // txtNameSpaceRoot
            // 
            this.txtNameSpaceRoot.Location = new System.Drawing.Point(214, 122);
            this.txtNameSpaceRoot.Name = "txtNameSpaceRoot";
            this.txtNameSpaceRoot.Size = new System.Drawing.Size(304, 26);
            this.txtNameSpaceRoot.TabIndex = 7;
            this.txtNameSpaceRoot.Text = "Evolution";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(156, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Create Service Class";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 250);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(147, 20);
            this.label6.TabIndex = 12;
            this.label6.Text = "Create Model Class";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 286);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(181, 20);
            this.label7.TabIndex = 14;
            this.label7.Text = "Create ViewModel Class";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 210);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 20);
            this.label8.TabIndex = 10;
            this.label8.Text = "Service Name";
            // 
            // chkCreateServiceClass
            // 
            this.chkCreateServiceClass.AutoSize = true;
            this.chkCreateServiceClass.Location = new System.Drawing.Point(214, 174);
            this.chkCreateServiceClass.Name = "chkCreateServiceClass";
            this.chkCreateServiceClass.Size = new System.Drawing.Size(22, 21);
            this.chkCreateServiceClass.TabIndex = 9;
            this.chkCreateServiceClass.UseVisualStyleBackColor = true;
            // 
            // txtServiceName
            // 
            this.txtServiceName.Location = new System.Drawing.Point(214, 210);
            this.txtServiceName.Name = "txtServiceName";
            this.txtServiceName.Size = new System.Drawing.Size(304, 26);
            this.txtServiceName.TabIndex = 11;
            // 
            // chkCreateModelClass
            // 
            this.chkCreateModelClass.AutoSize = true;
            this.chkCreateModelClass.Location = new System.Drawing.Point(214, 249);
            this.chkCreateModelClass.Name = "chkCreateModelClass";
            this.chkCreateModelClass.Size = new System.Drawing.Size(22, 21);
            this.chkCreateModelClass.TabIndex = 13;
            this.chkCreateModelClass.UseVisualStyleBackColor = true;
            // 
            // chkCreateViewModelClass
            // 
            this.chkCreateViewModelClass.AutoSize = true;
            this.chkCreateViewModelClass.Location = new System.Drawing.Point(214, 285);
            this.chkCreateViewModelClass.Name = "chkCreateViewModelClass";
            this.chkCreateViewModelClass.Size = new System.Drawing.Size(22, 21);
            this.chkCreateViewModelClass.TabIndex = 15;
            this.chkCreateViewModelClass.UseVisualStyleBackColor = true;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(19, 342);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(111, 45);
            this.btnCreate.TabIndex = 16;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 399);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.chkCreateViewModelClass);
            this.Controls.Add(this.chkCreateModelClass);
            this.Controls.Add(this.txtServiceName);
            this.Controls.Add(this.chkCreateServiceClass);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtNameSpaceRoot);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtLinqVarName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtClassNamePlural);
            this.Controls.Add(this.txtClassNameSingular);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "frmMain";
            this.Text = "Model Creator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtClassNameSingular;
        private System.Windows.Forms.TextBox txtClassNamePlural;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLinqVarName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtNameSpaceRoot;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkCreateServiceClass;
        private System.Windows.Forms.TextBox txtServiceName;
        private System.Windows.Forms.CheckBox chkCreateModelClass;
        private System.Windows.Forms.CheckBox chkCreateViewModelClass;
        private System.Windows.Forms.Button btnCreate;
    }
}

