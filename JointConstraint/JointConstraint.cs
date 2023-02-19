using UnityEngine;
using VMC;
using VMCMod;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;

namespace JointConstraint
{
    [VMCPlugin(
    Name: "JointConstraint",
    Version: "0.0.1",
    Author: "snow1226",
    Description: "Constraintを追加する",
    AuthorURL: "https://twitter.com/snow_mil",
    PluginURL: "https://github.com/Snow1226")]
    public class JointConstraint : MonoBehaviour
    {
        private VRIK _vrik;
        private IKSolver _ikSolver;
        private GameObject _currentModel;

        private List<ConstraintObject> _positionConstraints;
        private List<string> _positionConstraintNames = new List<string>()
        {
            "lower leg.L", "knee.L.001",
            "lower leg.R", "knee.R.001"
        };
        private List<ConstraintObject> _constraintObjects;
        private List<string> _constraintNames = new List<string>() 
        {
            "knee.L", "lower leg.L",
            "knee.R", "lower leg.R",
            "hips_support_L", "upper leg.L",
            "hips_support_R", "upper leg.R",
        };

        /*
            "upper_twist.L", "lower arm.L",
            "lower_twist.L", "hand.L",
            "upper_twist.R", "lower arm.R",
            "lower_twist.R", "hand.R"
        */

        public class ConstraintObject
        {
            public GameObject Source;
            public GameObject Target;
        }

        private void Awake()
        {
            VMCEvents.OnModelLoaded += OnModelLoaded;
            VMCEvents.OnModelLoaded += model => _currentModel = model;
        }
        void Start()
        {
            Debug.Log("JointConstraint Mod started.");
            _constraintObjects = new List<ConstraintObject>();
            _positionConstraints = new List<ConstraintObject>();
        }

        private void Update()
        {
            if (_vrik == null && _currentModel != null)
            {
                if (_ikSolver != null) _ikSolver.OnPostUpdate -= OnPostUpdate;
                _vrik = _currentModel.GetComponent<VRIK>();
                if (_vrik == null) return;
                _ikSolver = _vrik.GetIKSolver();
                _ikSolver.OnPostUpdate += OnPostUpdate;
            }
        }


        private void OnPostUpdate()
        {
            foreach(ConstraintObject cObj in _constraintObjects)
                RunConstraint(cObj.Source, cObj.Target);

            foreach (ConstraintObject cObj in _positionConstraints)
                cObj.Source.transform.position = cObj.Target.transform.position;
        }

        private void RunConstraint(GameObject src, GameObject target, float weight = 0.5f)
        {
            src.transform.localRotation = Quaternion.Slerp(Quaternion.identity, target.transform.localRotation, weight);
        }

        [OnSetting]
        public void OnSetting()
        {
        }

        private void OnModelLoaded(GameObject currentModel)
        {
            if (currentModel == null) return;

            var animator = currentModel.GetComponent<Animator>();
            StartCoroutine(SetConstraintChildObject());
        }

        private IEnumerator SetConstraintChildObject()
        {
            yield return new WaitForSeconds(1.0f);

            for(int i = 0; i< _constraintNames.Count; i += 2)
            {
                ConstraintObject cObj = new ConstraintObject()
                {
                    Source = GameObject.Find(_constraintNames[i]),
                    Target = GameObject.Find(_constraintNames[i + 1])
                };
                if(cObj.Source != null && cObj.Target!=null)
                    _constraintObjects.Add(cObj);
            }

            for (int i = 0; i < _positionConstraintNames.Count; i += 2)
            {
                ConstraintObject cObj = new ConstraintObject()
                {
                    Source = GameObject.Find(_positionConstraintNames[i]),
                    Target = GameObject.Find(_positionConstraintNames[i + 1])
                };
                if (cObj.Source != null && cObj.Target != null)
                    _positionConstraints.Add(cObj);
            }

        }
    }
}
