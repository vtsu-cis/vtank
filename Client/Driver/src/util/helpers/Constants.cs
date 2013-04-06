/*!
    \file   VTankGlobal.cs
    \brief  Global variables used in VTank.
    \author (C)Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;

namespace Client.src.util
{
    public static class Constants
    {
        public readonly static int MAX_PLAYER_HEALTH = 200;

        public readonly static int MAX_BASE_HEALTH = 600;

        //Charge-up weapons cap at a 4 second charge
        public readonly static float MAX_CHARGE = 3000;

        //Directory where the 3D models are held
        public readonly static string TANK_DIR = "tanks\\";

        //Directory where the 3D weapons are held
        public readonly static string TURRET_DIR = "weapons\\";

        public readonly static string PROJECTILE_DIR = "projectiles\\";

        public readonly static string MAP_DIR = "textures\\misc\\Map\\";

        public readonly static string CD_DIR = "textures\\misc\\CD\\";

        //Directory where the tile textures are held
        public readonly static string TILE_DIR = "textures\\misc\\tiles\\";

        //Directory where the background textures are held
        public readonly static string IMAGES_DIR = "textures\\misc\\background\\";

        //Diectory where the explosion textures are held
        public readonly static string EXPLOSION_DIR = "textures\\misc\\explosions\\";

        //Directory where the audio is held
        public readonly static string AUDIO_DIR = "Content\\audio\\";

        //Directory where the rank textures are held
        public readonly static string RANK_DIR = "ranks\\";

        // Directory where skins are held.
        public readonly static string DEFAULT_SKIN_DIR = @"models\tanks\skins\";

        // Default skin.
        public readonly static string DEFAULT_SKIN = "camo-tan";

        //Defines how far away the camera will be from the tank
        public readonly static float DEFAULT_CAMERA_HEIGHT = 800.0f;

        //Defines how large the message box will be
        //hight of 100 pixels
        //width of 400 pixels
        public readonly static Vector2 MESSAGE_BOX_SIZE_100 = new Vector2(400, 100);

        //Defines how large the message box will be
        //hight of 200 pixels
        //width of 400 pixels
        public readonly static Vector2 MESSAGE_BOX_SIZE_200 = new Vector2(400, 200);

        //Scale value for the 3D models
        public readonly static float SCALE = 0.14f;

        //Size of the tiles in pixels
        public readonly static int TILE_SIZE = 64;

        //! Detect collisions between tanks and walls for *other* players.
        public readonly static bool DETECT_NONLOCAL_COLLISION = true;

        public readonly static float MAX_FRIENDLY_TANK_DISTANCE = 4000f;

        //Scale value for the UI components
        public static float UI_SCALE = .60f;

        //The name of the mesh that turrets attach to
        public readonly static string TURRET_MOUNT = "Mount";

        //Sets the default tank velocity and angular velocity pixels/second
        public readonly static float TANK_VELOCITY = 275.0f;
        public readonly static float TANK_ANGULAR_VELOCITY = 2.666666667f;
    }
}
