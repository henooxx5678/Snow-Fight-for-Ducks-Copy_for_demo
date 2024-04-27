using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class PlayerBuildPreviewManager : MonoBehaviour {

        [System.Serializable]
        public class SnowWallPreviews {
            public SpriteRenderer left;
            public SpriteRenderer leftDown;
            public SpriteRenderer rightDown;
            public SpriteRenderer right;
            public SpriteRenderer rightUp;
            public SpriteRenderer leftUp;

            public SpriteRenderer GetByFacingDir (SnowWall.FacingDirection facingDir) {
                if (facingDir == SnowWall.FacingDirection.Left)
                    return left;
                else if (facingDir == SnowWall.FacingDirection.LeftDown)
                    return leftDown;
                else if (facingDir == SnowWall.FacingDirection.RightDown)
                    return rightDown;
                else if (facingDir == SnowWall.FacingDirection.Right)
                    return right;
                else if (facingDir == SnowWall.FacingDirection.RightUp)
                    return rightUp;
                else if (facingDir == SnowWall.FacingDirection.LeftUp)
                    return leftUp;

                return null;
            }
        }

        const int PREVIEWS_LAYER = 14;
        const int OBSTACLE_RIGIDBODY_LAYER = 10;

        public SnowWallPreviews snowWallPreviews;
        public Transform        previewCollidersParent;
        public Rigidbody2D      previewRigidbody;

        public Color illegalColor;


        public bool IsBuildLegal => _isBuildLegal;


        bool _isPreviewShowing = false;
        bool _isBuildingBaseShowing = false;
        bool _isBuildLegal = false;
        SnowWall.FacingDirection _currentShowingFacing;

        void Awake () {

            foreach (SnowWall.FacingDirection facingDir in System.Enum.GetValues(typeof(SnowWall.FacingDirection))) {
                SpriteRenderer sr = snowWallPreviews.GetByFacingDir(facingDir);
                sr.transform.position = Global.GetActualWorldPosition((Vector2) sr.transform.parent.position + SnowWall.GetVectorByFacingDir(facingDir) * Global.CurrentRoundInstance.playerProps.buildDistance);
                sr.enabled = false;

                previewRigidbody.transform.position = previewRigidbody.transform.parent.position + Vector3.right * Global.CurrentRoundInstance.playerProps.buildDistance;
                previewRigidbody.simulated = false;
            }
        }

        void Update () {

            if (_isBuildingBaseShowing) {
                SetPreviewCollidersLayer(OBSTACLE_RIGIDBODY_LAYER);
                SetColorAndEnable(_currentShowingFacing, Global.gameSceneManager.playerBuildingBaseOpacity);
                _isBuildingBaseShowing = false;
            }
            else if (_isPreviewShowing) {
                SetPreviewCollidersLayer(PREVIEWS_LAYER);
                SetColorAndEnable(_currentShowingFacing, Global.gameSceneManager.playerBuildPreviewOpacity, _isBuildLegal);
                _isPreviewShowing = false;
            }
            else {
                SpriteRenderer sr = snowWallPreviews.GetByFacingDir(_currentShowingFacing);
                sr.enabled = false;

                previewRigidbody.simulated = false;
            }

            foreach (SnowWall.FacingDirection facingDir in System.Enum.GetValues(typeof(SnowWall.FacingDirection))) {
                if (facingDir != _currentShowingFacing) {
                    SpriteRenderer sr = snowWallPreviews.GetByFacingDir(facingDir);
                    sr.enabled = false;
                }
            }
        }

        void FixedUpdate () {
            _isBuildLegal = true;
        }

        void SetPreviewCollidersLayer (int layer) {
            for (int i = 0 ; i < previewRigidbody.transform.childCount ; i++) {
                previewRigidbody.transform.GetChild(i).gameObject.layer = layer;
            }
        }

        void SetColorAndEnable (SnowWall.FacingDirection facing, float opacity, bool isBuildLegal = true) {
            SpriteRenderer sr = snowWallPreviews.GetByFacingDir(facing);

            if (isBuildLegal)
                sr.color = Color.white;
            else
                sr.color = illegalColor;

            sr.color = SetOpacity(sr.color, opacity);
            sr.enabled = true;

            previewCollidersParent.rotation = SnowWall.GetRotationByFacingDir(facing);
            previewRigidbody.simulated = true;
        }

        Color SetOpacity (Color color, float opacity) {
            color.a = opacity;
            return color;
        }


        public void ShowPreview (SnowWall.FacingDirection facing) {
            _isPreviewShowing = true;
            _currentShowingFacing = facing;
        }

        public void ShowBuildingBase (SnowWall.FacingDirection facing) {
            _isBuildingBaseShowing = true;
            _currentShowingFacing = facing;
        }

        public GameObject GenerateTempBuiltShowing (SnowWall.FacingDirection facing) {
            SpriteRenderer sr = snowWallPreviews.GetByFacingDir(facing);
            
            GameObject tempBuiltShowing = Instantiate(sr.gameObject, sr.transform.position, Quaternion.identity, Global.CurrentRoundInstance.transform);
            tempBuiltShowing.transform.localScale = sr.transform.lossyScale;

            SpriteRenderer tempBuiltShowingSR = tempBuiltShowing.GetComponent<SpriteRenderer>();
            tempBuiltShowingSR.color = SetOpacity(tempBuiltShowingSR.color, 1f);
            tempBuiltShowingSR.enabled = true;

            return tempBuiltShowing;
        }

        public void IllegalToBuild () {
            // call after FixedUpdate()
            _isBuildLegal = false;
        }

    }
}
