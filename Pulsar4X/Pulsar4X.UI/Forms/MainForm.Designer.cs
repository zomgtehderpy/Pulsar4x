﻿namespace Pulsar4X.UI.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.m_oMainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spaceMasterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sMOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sMOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.empireToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_oToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.m_oMainToolStrip = new System.Windows.Forms.ToolStrip();
            this.m_oSystemMapToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.m_oSystemViewToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_oMainMenuStrip.SuspendLayout();
            this.m_oToolStripContainer.TopToolStripPanel.SuspendLayout();
            this.m_oToolStripContainer.SuspendLayout();
            this.m_oMainToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_oMainMenuStrip
            // 
            this.m_oMainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem,
            this.spaceMasterToolStripMenuItem,
            this.empireToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.m_oMainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.m_oMainMenuStrip.Name = "m_oMainMenuStrip";
            this.m_oMainMenuStrip.Size = new System.Drawing.Size(1008, 24);
            this.m_oMainMenuStrip.TabIndex = 0;
            this.m_oMainMenuStrip.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.G)));
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.gameToolStripMenuItem.Text = "&Game";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // spaceMasterToolStripMenuItem
            // 
            this.spaceMasterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sMOnToolStripMenuItem,
            this.sMOffToolStripMenuItem});
            this.spaceMasterToolStripMenuItem.Name = "spaceMasterToolStripMenuItem";
            this.spaceMasterToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
            this.spaceMasterToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.spaceMasterToolStripMenuItem.Text = "&Space Master";
            // 
            // sMOnToolStripMenuItem
            // 
            this.sMOnToolStripMenuItem.Name = "sMOnToolStripMenuItem";
            this.sMOnToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.sMOnToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.sMOnToolStripMenuItem.Text = "SM On";
            // 
            // sMOffToolStripMenuItem
            // 
            this.sMOffToolStripMenuItem.Name = "sMOffToolStripMenuItem";
            this.sMOffToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.sMOffToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.sMOffToolStripMenuItem.Text = "SM Off";
            // 
            // empireToolStripMenuItem
            // 
            this.empireToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemMapToolStripMenuItem,
            this.systemInformationToolStripMenuItem});
            this.empireToolStripMenuItem.Name = "empireToolStripMenuItem";
            this.empireToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
            this.empireToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.empireToolStripMenuItem.Text = "&Empire";
            // 
            // systemMapToolStripMenuItem
            // 
            this.systemMapToolStripMenuItem.Name = "systemMapToolStripMenuItem";
            this.systemMapToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.systemMapToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.systemMapToolStripMenuItem.Text = "System Map";
            // 
            // systemInformationToolStripMenuItem
            // 
            this.systemInformationToolStripMenuItem.Name = "systemInformationToolStripMenuItem";
            this.systemInformationToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.systemInformationToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.systemInformationToolStripMenuItem.Text = "System Information";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // m_oToolStripContainer
            // 
            // 
            // m_oToolStripContainer.ContentPanel
            // 
            this.m_oToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1008, 681);
            this.m_oToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_oToolStripContainer.Location = new System.Drawing.Point(0, 24);
            this.m_oToolStripContainer.Name = "m_oToolStripContainer";
            this.m_oToolStripContainer.Size = new System.Drawing.Size(1008, 706);
            this.m_oToolStripContainer.TabIndex = 1;
            this.m_oToolStripContainer.Text = "toolStripContainer1";
            // 
            // m_oToolStripContainer.TopToolStripPanel
            // 
            this.m_oToolStripContainer.TopToolStripPanel.Controls.Add(this.m_oMainToolStrip);
            // 
            // m_oMainToolStrip
            // 
            this.m_oMainToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.m_oMainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.m_oSystemMapToolStripButton,
            this.m_oSystemViewToolStripButton});
            this.m_oMainToolStrip.Location = new System.Drawing.Point(3, 0);
            this.m_oMainToolStrip.Name = "m_oMainToolStrip";
            this.m_oMainToolStrip.Size = new System.Drawing.Size(95, 25);
            this.m_oMainToolStrip.TabIndex = 0;
            // 
            // m_oSystemMapToolStripButton
            // 
            this.m_oSystemMapToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_oSystemMapToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("m_oSystemMapToolStripButton.Image")));
            this.m_oSystemMapToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_oSystemMapToolStripButton.Name = "m_oSystemMapToolStripButton";
            this.m_oSystemMapToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.m_oSystemMapToolStripButton.Text = "System Map";
            // 
            // m_oSystemViewToolStripButton
            // 
            this.m_oSystemViewToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_oSystemViewToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("m_oSystemViewToolStripButton.Image")));
            this.m_oSystemViewToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_oSystemViewToolStripButton.Name = "m_oSystemViewToolStripButton";
            this.m_oSystemViewToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.m_oSystemViewToolStripButton.Text = "System View";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.m_oToolStripContainer);
            this.Controls.Add(this.m_oMainMenuStrip);
            this.MainMenuStrip = this.m_oMainMenuStrip;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "MainForm";
            this.Text = "Pulsar4X";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.m_oMainMenuStrip.ResumeLayout(false);
            this.m_oMainMenuStrip.PerformLayout();
            this.m_oToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.m_oToolStripContainer.TopToolStripPanel.PerformLayout();
            this.m_oToolStripContainer.ResumeLayout(false);
            this.m_oToolStripContainer.PerformLayout();
            this.m_oMainToolStrip.ResumeLayout(false);
            this.m_oMainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip m_oMainMenuStrip;
        private System.Windows.Forms.ToolStripContainer m_oToolStripContainer;
        private System.Windows.Forms.ToolStrip m_oMainToolStrip;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spaceMasterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem empireToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton m_oSystemMapToolStripButton;
        private System.Windows.Forms.ToolStripButton m_oSystemViewToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem sMOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sMOffToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}