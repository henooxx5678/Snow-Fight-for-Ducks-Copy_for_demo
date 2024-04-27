using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace DoubleHeat.Utilities {

    public static class VectorExtensions {

        public static Vector2 GetAfterSetX (this Vector2 v, float x) {
            v.x = x;
            return v;
        }

        public static Vector3 GetAfterSetX (this Vector3 v, float x) {
            v.x = x;
            return v;
        }

        public static Vector2 GetAfterSetY (this Vector2 v, float y) {
            v.y = y;
            return v;
        }

        public static Vector3 GetAfterSetY (this Vector3 v, float y) {
            v.y = y;
            return v;
        }

        public static Vector3 GetAfterSetZ (this Vector3 v, float z) {
            v.z = z;
            return v;
        }

        // Color
        public static Color GetAfterSetA (this Color c, float a) {
            c.a = a;
            return c;
        }



        public static Vector2 DirectionTo (this Vector2 v0, Vector2 v1) {
            return (v1 - v0).normalized;
        }

        public static Vector3 DirectionTo (this Vector3 v0, Vector3 v1) {
            return (v1 - v0).normalized;
        }


        public static Vector2 GetRotateTowards (this Vector2 currentDir, Vector2 destinationDir, float maxAngleDelta) {
            float destinationRotAngle = Vector2.SignedAngle(currentDir, destinationDir);
            float destinationRotDir   = Mathf.Sign(destinationRotAngle);
            float actualRotAngle = destinationRotDir * Mathf.Min( Mathf.Abs(destinationRotAngle), maxAngleDelta );

            return Quaternion.AngleAxis(actualRotAngle, Vector3.forward) * currentDir;
        }
    }

    public static class TransformExtensions {

        public static void SetPosX (this Transform transform, float x) {
            Vector3 pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }

        public static void SetPosY (this Transform transform, float y) {
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }

        public static void SetPosZ (this Transform transform, float z) {
            Vector3 pos = transform.position;
            pos.z = z;
            transform.position = pos;
        }

        public static void SetPosXY (this Transform transform, Vector2 v2) {
            Vector3 pos = transform.position;
            pos.x = v2.x;
            pos.y = v2.y;
            transform.position = pos;
        }

        public static void SetLocalPosX (this Transform transform, float x) {
            Vector3 pos = transform.localPosition;
            pos.x = x;
            transform.localPosition = pos;
        }

        public static void SetLocalPosY (this Transform transform, float y) {
            Vector3 pos = transform.localPosition;
            pos.y = y;
            transform.localPosition = pos;
        }

        public static void SetLocalPosZ (this Transform transform, float z) {
            Vector3 pos = transform.localPosition;
            pos.z = z;
            transform.localPosition = pos;
        }

        public static void SetLocalPosXY (this Transform transform, Vector2 v2) {
            Vector3 pos = transform.localPosition;
            pos.x = v2.x;
            pos.y = v2.y;
            transform.localPosition = pos;
        }

        public static void SetScaleX (this Transform transform, float x) {
            Vector3 scale = transform.localScale;
            scale.x = x;
            transform.localScale = scale;
        }

        public static void SetScaleY (this Transform transform, float y) {
            Vector3 scale = transform.localScale;
            scale.y = y;
            transform.localScale = scale;
        }

        public static void SetScaleZ (this Transform transform, float z) {
            Vector3 scale = transform.localScale;
            scale.z = z;
            transform.localScale = scale;
        }

        public static void SetTransformValuesNonLocal (this Transform thisTrans, Transform targetTrans) {
            thisTrans.position   = targetTrans.position;
            thisTrans.rotation   = targetTrans.rotation;
            thisTrans.localScale = targetTrans.localScale;
        }

        public static void SetTransformValuesLocal (this Transform thisTrans, Transform targetTrans) {
            thisTrans.localPosition = targetTrans.localPosition;
            thisTrans.localRotation = targetTrans.localRotation;
            thisTrans.localScale    = targetTrans.localScale;
        }


        public static void SetAnchoredPosX (this RectTransform rectTransform, float x) {
            Vector3 pos = rectTransform.anchoredPosition;
            pos.x = x;
            rectTransform.anchoredPosition = pos;
        }

        public static void SetAnchoredPosY (this RectTransform rectTransform, float y) {
            Vector3 pos = rectTransform.anchoredPosition;
            pos.y = y;
            rectTransform.anchoredPosition = pos;
        }


        public static void SetRectTransformValues (this RectTransform thisRectTrans, RectTransform targetRectTrans) {
            thisRectTrans.anchorMin        = targetRectTrans.anchorMin;
            thisRectTrans.anchorMax        = targetRectTrans.anchorMax;
            thisRectTrans.anchoredPosition = targetRectTrans.anchoredPosition;
            thisRectTrans.sizeDelta        = targetRectTrans.sizeDelta;
        }


        public static void SetLayerRecursively (this Transform thisTrans, int layer) {
            thisTrans.gameObject.layer = layer;

            foreach (Transform child in thisTrans) {
                if (child != null) {
                    child.SetLayerRecursively(layer);
                }
            }
        }
    }

    public static class RendererExtensions {

        public static bool IsInViewport (this RectTransform rectTrans, Camera cam) {

            FloatRange insideRange = new FloatRange(0f, 1f);

            Vector3[] corners = new Vector3[4];
            rectTrans.GetWorldCorners(corners);

            foreach (Vector3 corner in corners) {
                Vector3 viewportPoint = cam.WorldToScreenPoint(corner);
                if (insideRange.IsInRange(viewportPoint.x) && insideRange.IsInRange(viewportPoint.y))
                    return true;
            }
            return false;
        }

        public static void SetSortingLayer (this SpriteRenderer sr, string layerName) {
            sr.sortingLayerName = layerName;
            sr.sortingLayerID = SortingLayer.NameToID(layerName);
        }

        public static void SetSortingLayer (this Canvas canvas, string layerName) {
            canvas.sortingLayerName = layerName;
            canvas.sortingLayerID = SortingLayer.NameToID(layerName);
        }

        public static void SetPivotToSpritePivot (this RectTransform rectTrans, Sprite sprite) {
            Vector2 size = sprite.rect.size;
            Vector2 pixelPivot = sprite.pivot;
            Vector2 percentPivot = new Vector2(pixelPivot.x / size.x, pixelPivot.y / size.y);
            rectTrans.pivot = percentPivot;
        }


        public static void SetOpacity (this SpriteRenderer sr, float alpha) {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }

        public static void SetOpacity (this Tilemap tilemap, float alpha) {
            Color color = tilemap.color;
            color.a = alpha;
            tilemap.color = color;
        }
    }


    public static class Vector2Tools {

        public static float Direction2DToAngle (Vector2 v) {
            return Vector2.SignedAngle(Vector2.right, v);
        }

        public static Vector2 AngleToDirection2D (float angle) {
            return Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        }

        // public static Vector2 MidDirection2d


        public static Vector2 GetClosestPointInSegment (this Vector2 source, Vector2[] segment) {
            Vector2 segmentDir = (segment[1] - segment[0]).normalized;
            float segmentDistance = Vector2.Dot(segment[1] - segment[0], segmentDir);

            return segment[0] + Mathf.Clamp(Vector2.Dot(source - segment[0], segmentDir), 0f, segmentDistance) * segmentDir;
        }

    }
    

    public static class PhysicsTools2D {

        public static Vector2 GetFinalDeltaPosAwaringObstacle (Rigidbody2D rigidbody, Vector2 dir, float distance, LayerMask mask) {

            // Collide With Obstacles (Detect)
            List<RaycastHit2D> hits    = new List<RaycastHit2D>();
            ContactFilter2D    filter  = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(mask);;

            Vector2 finalDeltaPos = dir * distance;

            if (rigidbody.Cast(dir, filter, hits, distance) > 0) {

                Vector2 totalComp = Vector2.zero;

                foreach (var hit in hits) {

                    float totalCompProjectLength = Vector2.Dot(totalComp, hit.normal);
                    float compensationLength = Vector2.Dot(dir * (hit.distance - distance), hit.normal) - totalCompProjectLength;

                    if (compensationLength > 0) {
                        Vector2 compensation = compensationLength * hit.normal;
                        totalComp += compensation;
                    }
                }

                finalDeltaPos += totalComp;
            }

            return finalDeltaPos;
        }

    }

    public static class ComponentsTools {

        public static void SetAndKeepAttachedGameObjectUniquely<T> (ref T container, T newSetted) where T : MonoBehaviour {
            if (container != null && container.gameObject != null)
                Object.Destroy(container.gameObject);

            container = newSetted;
        }
    }


}
