using System;
using UnityEngine;

namespace CharacterCustomizationTool.FaceManagement
{
    public class FacePicker : MonoBehaviour
    {
        [SerializeField] private FaceType _activeFace;
        [SerializeField] private Mesh[] _faces = Array.Empty<Mesh>();

        public FaceType ActiveFace => _activeFace;

        public void SetFaces(Mesh[] faces)
        {
            _faces = faces ?? Array.Empty<Mesh>();

            if (!HasFace(_activeFace) && _faces.Length > 0)
            {
                _activeFace = FaceType.Face0;
            }

            ApplyFace();
        }

        public bool HasFace(FaceType face)
        {
            var index = (int)face;
            return index >= 0 && index < _faces.Length && _faces[index] != null;
        }

        public void PickFace(FaceType face)
        {
            if (!HasFace(face))
            {
                return;
            }

            _activeFace = face;
            ApplyFace();
        }

        private void ApplyFace()
        {
            var index = (int)_activeFace;
            if (index < 0 || index >= _faces.Length)
            {
                return;
            }

            var mesh = _faces[index];
            if (mesh == null)
            {
                return;
            }

            var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
            }
        }
    }
}