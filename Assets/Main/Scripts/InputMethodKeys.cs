using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {


    public class InputMethodKeys {

        public enum GadgetSwitchMethod {
            CorrespondingKey,
            LeftRightSwitch,
            SelectionsRing
        }

        public InputDevice device;

        public KeyCode advance;
        public KeyCode back;
        public KeyCode menu;
        public KeyCode inGameInfo;

        public KeyCode fire;
        public KeyCode secondaryFire;
        public KeyCode collect;
        public KeyCode quack;
        public KeyCode curveBallSwitchDirection;

        public GadgetSwitchMethod[] gadgetSwitchMethods;
        public KeyCode[] gadgets;
        public KeyCode   switchGadgetLeft;
        public KeyCode   switchGadgetRight;
        public KeyCode   holdToSwitchGadget;


        public static InputMethodKeys[] keyboardMouseLayout = new InputMethodKeys[] {
            new InputMethodKeys {
                advance = KeyCode.Return,
                back = KeyCode.Escape,
                menu = KeyCode.Escape,
                inGameInfo = KeyCode.Tab,

                fire = KeyCode.Mouse0,
                secondaryFire = KeyCode.Mouse1,
                collect = KeyCode.Space,
                quack = KeyCode.LeftShift,
                curveBallSwitchDirection = KeyCode.Alpha1,

                gadgetSwitchMethods = new GadgetSwitchMethod[] {
                    GadgetSwitchMethod.CorrespondingKey,
                    GadgetSwitchMethod.LeftRightSwitch
                },
                gadgets = new KeyCode[] {
                    KeyCode.Alpha1,
                    KeyCode.Alpha2,
                    KeyCode.Alpha3,
                    KeyCode.Alpha4
                },
                switchGadgetLeft = KeyCode.Q,
                switchGadgetRight = KeyCode.E
            },
            new InputMethodKeys {
                advance = KeyCode.Return,
                back = KeyCode.Escape,
                menu = KeyCode.Escape,
                inGameInfo = KeyCode.Tab,

                fire = KeyCode.Mouse0,
                secondaryFire = KeyCode.Mouse1,
                collect = KeyCode.E,
                quack = KeyCode.Q,
                curveBallSwitchDirection = KeyCode.Alpha1,

                gadgetSwitchMethods = new GadgetSwitchMethod[] {
                    GadgetSwitchMethod.CorrespondingKey
                },
                gadgets = new KeyCode[] {
                    KeyCode.Alpha1,
                    KeyCode.Alpha2,
                    KeyCode.Alpha3,
                    KeyCode.Alpha4
                }
            }
        };

        // gamepad layout




        public string GetKeyIconLabel (KeyCode key) {

            string spritesFileName = "";

            if (device == InputDevice.KeyboardMouse) {
                spritesFileName = "Keyboard Mouse Icon";
            }
            else if (device == InputDevice.XboxOneController) {
                spritesFileName = "Xbox One Controller Icon";
            }

            return string.Format("<sprite=\"{0}\" name=\"{1}\">", spritesFileName, GetSpriteName(key));
        }

        string GetSpriteName (KeyCode key) {

            string spriteName;

            if (device == InputDevice.KeyboardMouse) {
                if (Global.inlineIconKeyboardMouseCorrespondence.TryGetValue(key, out spriteName))
                    return spriteName;
            }
            else if (device == InputDevice.XboxOneController) {
                if (Global.inlineIconXboxOneControllerCorrespondence.TryGetValue(key, out spriteName))
                    return spriteName;
            }

            return "";
        }


    }

}
