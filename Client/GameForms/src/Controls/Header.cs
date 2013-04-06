using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Controls
{
    class Header : Control
    {
        List<HeaderButton> buttons;
        public List<HeaderButton> HeaderButtons { get { return buttons; } }
        public int ColumnCount { get { return buttons.Count; } }

    #region //// Consts ////////////

    ////////////////////////////////////////////////////////////////////////////        
    private const string skButton = "Custom.Button"; //Control's skin we use (from skin file)
    private const string lrButton = "Control"; // Layer we will draw (from skin file)
    ////////////////////////////////////////////////////////////////////////////

    #endregion    

    #region //// Construstors //////

    ////////////////////////////////////////////////////////////////////////////       
    public Header(Manager manager, int _columns, int width)
        : base(manager)
    {
        SetDefaultSize(width, 1);
        this.Width = width;
        int defaultWidth = width / _columns;
        buttons = new List<HeaderButton>();
        HeaderButton btn;

        for (int i = 0; i < _columns; i++)
        {
            btn = new HeaderButton(manager);
            btn.Init();
            btn.Top = 0;
            btn.Left = i * defaultWidth;
            btn.Width = defaultWidth;
            buttons.Add(btn);
            this.Add(btn);
        }
        this.Height = buttons[0].Height;
        this.Color = Color.DimGray;
    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Methods ///////////

    ////////////////////////////////////////////////////////////////////////////
    public override void Init()
    {
      base.Init();
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    protected override void InitSkin()
    {
      base.InitSkin();
      
      // We specify what skin this control uses.
      //Skin = new SkinControl(Manager.Skin.Controls[skButton]);
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
    {
      // We draw the control the same way the ancestor does.
      // In this case, ancestor draws all states automatically according to description in the skin file.
      base.DrawControl(renderer, rect, gameTime);

      //SkinLayer layer = Skin.Layers[lrButton];            
      //int ox = 0; int oy = 0;

      //// We want to have the text pushed a bit down, when we press the button
      //if (ControlState == ControlState.Pressed)
      //{        
      //  ox = 1; oy = 1;
      //}      
      
      //// Now we draw text over our control. This method does it automatically, 
      //// based on the description in the skin file. 
      //// We use our pressed-state offset for text position.
      //renderer.DrawString(this, layer, Text, rect, true, ox, oy);
    }
    ////////////////////////////////////////////////////////////////////////////         


    #endregion
    }


    class HeaderButton : ButtonBase
    {
        
    #region //// Consts ////////////

    ////////////////////////////////////////////////////////////////////////////        
    private const string skButton = "Custom.Button"; //Control's skin we use (from skin file)
    private const string lrButton = "Control"; // Layer we will draw (from skin file)
    ////////////////////////////////////////////////////////////////////////////

    #endregion    

    #region //// Construstors //////

    ////////////////////////////////////////////////////////////////////////////       
    public HeaderButton(Manager manager): base(manager)
    {
      // If not specified in skin file, we set these defaults.
      SetDefaultSize(72, 10);
    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Methods ///////////

    ////////////////////////////////////////////////////////////////////////////
    public override void Init()
    {
      base.Init();
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    protected override void InitSkin()
    {
      base.InitSkin();
      
      // We specify what skin this control uses.
      Skin = new SkinControl(Manager.Skin.Controls[skButton]);
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
    {
      // We draw the control the same way the ancestor does.
      // In this case, ancestor draws all states automatically according to description in the skin file.
      base.DrawControl(renderer, rect, gameTime);

      SkinLayer layer = Skin.Layers[lrButton];            
      int ox = 0; int oy = 0;

      // We want to have the text pushed a bit down, when we press the button
      if (ControlState == ControlState.Pressed)
      {        
        ox = 1; oy = 1;
      }      
      
      // Now we draw text over our control. This method does it automatically, 
      // based on the description in the skin file. 
      // We use our pressed-state offset for text position.
      renderer.DrawString(this, layer, Text, rect, true, ox, oy);
    }
    ////////////////////////////////////////////////////////////////////////////         

    #endregion
    }


}
