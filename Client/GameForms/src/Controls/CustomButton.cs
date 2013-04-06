////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Samples                                          //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: CustomButton.cs                              //
//                                                            //
//      Version: 0.0                                          //
//                                                            //
//         Date: 11/09/2008                                   //
//                                                            //
//       Author: Tom Shane                                    //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//  Copyright (c) by Tom Shane                                //
//                                                            //
////////////////////////////////////////////////////////////////


#region //// Using /////////////

////////////////////////////////////////////////////////////////////////////
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TomShane.Neoforce.Controls;
////////////////////////////////////////////////////////////////////////////

#endregion

namespace GameForms.Controls
{

  #region //// Classes ///////////
    
  ////////////////////////////////////////////////////////////////////////////  
  public class CustomButton: ButtonBase
  {

    #region //// Consts ////////////

    ////////////////////////////////////////////////////////////////////////////        
    private const string skButton = "Custom.Button"; //Control's skin we use (from skin file)
    private const string lrButton = "Control"; // Layer we will draw (from skin file)
    ////////////////////////////////////////////////////////////////////////////

    #endregion    

    #region //// Construstors //////

    ////////////////////////////////////////////////////////////////////////////       
    public CustomButton(Manager manager): base(manager)
    {
      // If not specified in skin file, we set these defaults.
      SetDefaultSize(72, 24);
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

  #endregion

}
