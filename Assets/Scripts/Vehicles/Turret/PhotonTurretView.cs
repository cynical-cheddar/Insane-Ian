// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using UnityEngine;

    [AddComponentMenu("Photon Networking/Photon Turret View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonTurretView : MonoBehaviourPun, IPunObservable
    {
        private float m_BodyAngle;
        private float m_BarrelAngle;

        private Quaternion m_NetworkBodyRotation;
        private Quaternion m_NetworkBarrelRotation;

        public Transform bodyTransform = null;
        public Transform barrelTransform = null;


        bool m_firstTake = false;

        public void Awake()
        {
            if (bodyTransform == null) bodyTransform = transform.Find("BodyCentre");
            if (barrelTransform == null) barrelTransform = bodyTransform.Find("BarrelHinge");

            m_NetworkBodyRotation = Quaternion.identity;
            m_NetworkBarrelRotation = Quaternion.identity;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {

            if (!this.photonView.IsMine)
            {
                var tr = bodyTransform;
                tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkBodyRotation, this.m_BodyAngle * (4.0f / PhotonNetwork.SerializationRate));

                tr = barrelTransform;
                tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkBarrelRotation, this.m_BarrelAngle * (4.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // Write
            if (stream.IsWriting)
            {
                stream.SendNext(bodyTransform.localRotation);
                stream.SendNext(barrelTransform.localRotation);
            }
            // Read
            else
            {
                this.m_NetworkBodyRotation = (Quaternion)stream.ReceiveNext();
                this.m_NetworkBarrelRotation = (Quaternion)stream.ReceiveNext();

                if (m_firstTake)
                {
                    this.m_BodyAngle = 0f;

                    bodyTransform.localRotation = this.m_NetworkBodyRotation;
                    barrelTransform.localRotation = this.m_NetworkBarrelRotation;
                }
                else
                {
                    this.m_BodyAngle = Quaternion.Angle(bodyTransform.localRotation, this.m_NetworkBodyRotation);
                    this.m_BarrelAngle = Quaternion.Angle(barrelTransform.localRotation, this.m_NetworkBarrelRotation);
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}