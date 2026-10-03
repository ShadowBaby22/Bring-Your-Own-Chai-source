using UnityEditor;

namespace KadakChai.EditorTools
{
    /// <summary>
    /// Everything under Resources/Art imports as ONE full-rect sprite with no mipmaps.
    /// (Unity 6 otherwise auto-slices images with separate pieces into "Multiple" sprites.)
    /// </summary>
    class ArtImportSettings : AssetPostprocessor
    {
        public override uint GetVersion() => 2;

        /// <summary>Sounds under Resources/Audio load up front, so the first play isn't lost while decoding (matters in browsers).</summary>
        void OnPreprocessAudio()
        {
            if (!assetPath.Contains("/Resources/Audio/")) return;
            var ai = (AudioImporter)assetImporter;
            var ss = ai.defaultSampleSettings;
            ss.preloadAudioData = true;
            ai.defaultSampleSettings = ss;
            ai.loadInBackground = false;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Resources/Art/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }
    }
}
