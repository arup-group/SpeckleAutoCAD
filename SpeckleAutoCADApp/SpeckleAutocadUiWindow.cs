using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleUiBase;
using System.ComponentModel;

namespace SpeckleUiBase
{
    public partial class SpeckleAutocadUiWindow : SpeckleUiWindow
    {
        public SpeckleAutocadUiWindow(SpeckleUIBindings baseBindings, string address = "https://matteo-dev.appui.speckle.systems/#/")
            : base(baseBindings, address)
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
