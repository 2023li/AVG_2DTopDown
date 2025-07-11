using System.Collections.Generic;
using System.Linq;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CollectionScripts;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.Utils;
using UnityEngine;
using UnityEngine.U2D.Animation;


namespace Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts
{
   

    public class CharacterBuilder : MonoBehaviour
    {
        public CostumeCharacter Character;
        public SpriteCollection SpriteCollection;
        public string Head = "Human";
        public string Ears = "Human";
        public string Eyes = "Human";
        public string Body = "Human";
        public string Hair;
        public string Armor;
        public string Helmet;
        public string Weapon;
        public string Firearm;
        public string Shield;
        public string Cape;
        public string Back;
        public string Mask;
        public string Horns;

        public Texture2D Texture { get; private set; }
        private Dictionary<string, Sprite> _sprites;
        //private string _hash;

        public void Awake()
        {
            Rebuild();
        }

        public void Rebuild(bool forceMerge = false)
        {
            //var hash = $"{Head}.{Ears}.{Eyes}.{Body}.{Hair}.{Armor}.{Helmet}.{Weapon}.{Firearm}.{Shield}.{Cape}.{Back}.{Mask}.{Horns}";

            //if (hash == _hash) return;

            //_hash = hash;

            //var width = SpriteCollection.Layers[0].Textures[0].width;
            //var height = SpriteCollection.Layers[0].Textures[0].height;
            var width = 576;
            var height = 928;
            var dict = SpriteCollection.Layers.ToDictionary(i => i.Name, i => i);
            Dictionary<string, Color32[]> layers = new Dictionary<string, Color32[]>();

            if (Head.Contains("Lizard")) Hair = Helmet = Mask = "";
            
            if (Back != "") layers.Add("Back", dict["Back"].GetPixels(Back, null));
            if (Shield != "") layers.Add("Shield", dict["Shield"].GetPixels(Shield, null));
            
            if (Body != "")
            {
                layers.Add("Body", dict["Body"].GetPixels(Body, null));

                if (Firearm == "")
                {
                    var arms = dict["Arms"].GetPixels(Body, null).ToArray();

                    layers.Add("Arms", arms);
                }
            }

            if (Head != "") layers.Add("Head", dict["Head"].GetPixels(Head, null));
            if (Ears != "" && (Helmet == "" || Helmet.Contains("[ShowEars]"))) layers.Add("Ears", dict["Ears"].GetPixels(Ears, null));

            if (Armor != "")
            {
                layers.Add("Armor", dict["Armor"].GetPixels(Armor, null));

                if (Firearm == "")
                {
                    layers.Add("Bracers", dict["Bracers"].GetPixels(Armor, null));
                }
            }

            if (Eyes != "") layers.Add("Eyes", dict["Eyes"].GetPixels(Eyes, null));
            if (Hair != "") layers.Add("Hair", dict["Hair"].GetPixels(Hair, Helmet == "" ? null : layers["Head"]));
            if (Cape != "") layers.Add("Cape", dict["Cape"].GetPixels(Cape, null));
            if (Helmet != "") layers.Add("Helmet", dict["Helmet"].GetPixels(Helmet, null));
            if (Weapon != "") layers.Add("Weapon", dict["Weapon"].GetPixels(Weapon, null));

            if (Firearm != "")
            {
                var firearm = dict["Firearm"].GetPixels(Firearm, null).ToArray();

                if (Character.Firearm.Detached && !forceMerge)
                {
                    for (var y = 0; y < height; y++)
                    {
                        if (y >= 0 * 64 && y < 1 * 64) continue; // Roll
                        if (y >= 1 * 64 && y < 2 * 64) continue; // Death
                        if (y >= 2 * 64 && y < 3 * 64) continue; // Block
                        if (y >= 5 * 64 && y < 6 * 64) continue; // Slash
                        if (y >= 6 * 64 && y < 7 * 64) continue; // Jab
                        if (y >= 9 * 64 && y < 10 * 64) continue; // Climb
                        if (y >= 13 * 64 && y < 14 * 64) continue; // Idle

                        for (var x = 0; x < width; x++)
                        {
                            firearm[x + y * width] = new Color32();
                        }
                    }
                }

                if (Armor != "" || Body != "") // Replace gloves color.
                {
                    var index = 27 + 844 * width;
                    var pixels = dict[Armor != "" ? "Bracers" : "Arms"].GetPixels(Armor != "" ? Armor : Body, null);

                    if (pixels != null)
                    {
                        var replacement = pixels[index];

                        if (replacement.a > 0)
                        {
                            var hand = new Color32(246, 202, 159, 255);

                            for (var i = 0; i < firearm.Length; i++)
                            {
                                if (firearm[i].FastEquals(hand)) firearm[i] = replacement;
                            }
                        }
                    }
                }
                
                layers.Add("Firearm", firearm);
            }

            if (Mask != "") layers.Add("Mask", dict["Mask"].GetPixels(Mask, null));
            if (Horns != "" && Helmet == "") layers.Add("Horns", dict["Horns"].GetPixels(Horns, null));

            var order = SpriteCollection.Layers.Select(i => i.Name).ToList();

            layers = layers.Where(i => i.Value != null).OrderBy(i => order.IndexOf(i.Key)).ToDictionary(i => i.Key, i => i.Value);

            if (Texture == null) Texture = new Texture2D(width, height) { filterMode = FilterMode.Point };

            if (Shield != "")
            {
                var shield = layers["Shield"];
                var last = layers.Last(i => i.Key != "Weapon");
                var copy = last.Value.ToArray();

                for (var i = 2 * 64 * width; i < 3 * 64 * width; i++)
                {
                    if (shield[i].a > 0) copy[i] = shield[i];
                }

                layers[last.Key] = copy;
            }

            if (Firearm != "")
            {
                foreach (var layerName in new[] { "Head", "Ears", "Eyes", "Mask", "Hair", "Helmet" })
                {
                    if (!layers.ContainsKey(layerName)) continue;

                    var copy = layers[layerName].ToArray();

                    for (var y = 11 * 64 - 1; y >= 10 * 64 - 1; y--)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            copy[x + y * width] = copy[x + (y - 1) * width];
                        }
                    }

                    layers[layerName] = copy;
                }
            }
            
            Texture = TextureHelper.MergeLayers(Texture, layers.Values.ToArray());
            Texture.SetPixels(0, Texture.height - 32, 32, 32, new Color[32 * 32]);

            if (Cape != "") CapeOverlay(layers["Cape"]);


            //--------------------------------制作动画部分------------------------------------
            if (_sprites == null)
            {
                List<string> clipNames = new List<string> { "Idle", "Ready", "Run", "Crawl", "Climb", "Jump", "Push", "Jab", "Slash", "Shot", "Fire", "Block", "Death", "Roll" };

                clipNames.Reverse();

                _sprites = new Dictionary<string, Sprite>();

                for (int i = 0; i < clipNames.Count; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        string key = clipNames[i] + "_" + j;

                        _sprites.Add(key, Sprite.Create(Texture, new Rect(j * 64, i * 64, 64, 64), new Vector2(0.5f, 0.125f), 16, 0, SpriteMeshType.FullRect));
                    }
                }
            }

           SpriteLibraryAsset spriteLibraryAsset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();

            foreach (var sprite in _sprites)
            {
                var split = sprite.Key.Split('_');

                spriteLibraryAsset.AddCategoryLabel(sprite.Value, split[0], split[1]);
            }

            Character.Body.GetComponent<SpriteLibrary>().spriteLibraryAsset = spriteLibraryAsset;

            if (Character.Firearm.Renderer != null)
            {
                
                if (Firearm == "")
                {
                    Character.Firearm.Renderer.enabled = false;
                }
                else
                {
                    Character.Firearm.Renderer.enabled = true;

                    var texture = new Texture2D(64, 64) { filterMode = FilterMode.Point };
                    var pixels = dict["Firearm"].GetPixels(Firearm, null);
                    
                    for (var x = 0; x < 64; x++)
                    {
                        for (var y = 0; y < 64; y++)
                        {
                            texture.SetPixel(x, y, pixels[x + (y + 12 * 64) * width]);
                        }
                    }

                    texture.Apply();

                    Character.Firearm.Renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.125f), 16);
                    Character.Firearm.FireMuzzlePosition = GetMuzzlePosition(texture);
                    Character.Firearm.FireMuzzle.localPosition = Character.Firearm.FireMuzzlePosition / 16;
                }
            }

            Character.Firearm.Renderer.gameObject.SetActive(Character.Firearm.Detached);
        }

        private void CapeOverlay(Color32[] cape)
        {
            if (Cape == "") return;
            
            var pixels = Texture.GetPixels32();
            var width = Texture.width;
            var height = Texture.height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    //if (x >= 0 && x < 2 * 64 && y >= 9 * 64 && y < 10 * 64 // "Climb_0", "Climb_1"
                    //    || x >= 64 && x < 64 + 2 * 64 && y >= 6 * 64 && y < 7 * 64 // "Jab_1", "Jab_2"
                    //    || x >= 128 && x < 128 + 2 * 64 && y >= 5 * 64 && y < 6 * 64 // "Slash_2", "Slash_3"
                    //    || x >= 0 && x < 4 * 64 && y >= 4 * 64 && y < 5 * 64) // "Shot_0", "Shot_1", "Shot_2", "Shot_3"
                    if (x >= 0 && x < 2 * 64 && y >= 9 * 64 && y < 10 * 64 // "Climb_0", "Climb_1"
                        || x >= 64 && x < 64 + 2 * 64 && y >= 6 * 64 && y < 7 * 64 // "Jab_1", "Jab_2"
                        || x >= 128 && x < 128 + 2 * 64 && y >= 5 * 64 && y < 6 * 64 // "Slash_2", "Slash_3"
                        || x >= 0 && x < 4 * 64 && y >= 4 * 64 && y < 5 * 64) // "Shot_0", "Shot_1", "Shot_2", "Shot_3"
                    {
                        var i = x + y * width;

                        if (cape[i].a > 0) pixels[i] = cape[i];
                    }
                }
            }

            Texture.SetPixels32(pixels);
            Texture.Apply();
        }

        /// <summary>
        /// 获取枪口位置
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        private static Vector2 GetMuzzlePosition(Texture2D texture)
        {
            var muzzlePosition = new Vector2(texture.width / 2f - 1, 6);

            for (var x = 63; x >= 0; x--)
            {
                for (var y = 0; y < 64; y++)
                {
                    if (texture.GetPixel(x, y).a > 0)
                    {
                        return muzzlePosition;
                    }
                }

                muzzlePosition.x = x - 1 - texture.width / 2f;
            }

            return muzzlePosition;
        }
    }
}