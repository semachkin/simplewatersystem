using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaterPhysics : MonoBehaviour
{
    public class WaterRegion {
        public Vector2 Min;
        public Vector2 Max;

        public float baseHeight;
        public float density;

        Transform Transf;

        public struct Wave {
            public Vector2 dir;
            public float lambda;
            public float stepness;

            public Wave(Vector4 wave) {
                dir = new Vector2(wave.x, wave.y).normalized;
                stepness = wave.z;
                lambda = wave.w;
            }
        }

        public Wave[] waves;
        public float gravity;

        public WaterRegion(Transform obj) {
            Water water = obj.gameObject.GetComponent<Water>();
            baseHeight = obj.position.y;
            density = water.waterDensity;

            Transf = obj;
            
            Vector2 pos = new Vector2(obj.position.x, obj.position.z);
            Vector2 size = new Vector2(water.width * water.cellSize, water.height * water.cellSize);

            Min = pos;
            Max = pos + size;

            Material waterMaterial = water.waterMaterial;

            waves = new Wave[4];

            for (int i = 0; i < 4; i++) {
                Vector4 wave = waterMaterial.GetVector($"_Wave{i + 1}");
                waves[i] = new Wave(wave);
            }

            gravity = waterMaterial.GetFloat("_G");
        }

        public bool isIn(Vector3 pos) => pos.x > Min.x && pos.z > Min.y && pos.x < Max.x && pos.z < Max.y;

        public override string ToString() {
            return Transf.gameObject.name;
        }

        public float Gerstner(Vector3 pos, ref Vector3 normal) {
            float offset = baseHeight;

            Vector3 basisFront = Vector3.zero;
            Vector3 basisRight = Vector3.zero;

            foreach (Wave wave in waves) {
                Vector3 dir = wave.dir;

                float k = 2 * Mathf.PI / wave.lambda;
                float c = Mathf.Sqrt(gravity / k); 
                float f = k * (dir.x * pos.x + dir.y * pos.z - Time.fixedTime * c);
                float a = wave.stepness / k;

                offset += a * Mathf.Sin(f);

                basisFront += new Vector3(
                    1 - dir.x * dir.x * (wave.stepness * Mathf.Sin(f)), 
                    dir.x * (wave.stepness * Mathf.Cos(f)), 
                    -dir.x * dir.y * (wave.stepness * Mathf.Sin(f))
                ).normalized;

                basisRight += new Vector3(
                    - dir.x * dir.y * (wave.stepness * Mathf.Sin(f)), 
                    dir.y * (wave.stepness * Mathf.Cos(f)), 
                    1 - dir.y * dir.y * (wave.stepness * Mathf.Sin(f))
                ).normalized;
            }

            normal = Vector3.Cross(basisRight, basisFront).normalized;

            return offset;
        }
    }
  
    public float density;
    [Range(0, 10)]
    public float damping;
    [Range(0, 5)]
    public float drag;

    static WaterRegion[] waterRegions;

    WaterRegion currentRegion;
    
    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Awake() {
        waterRegions = Array.ConvertAll(GameObject.FindGameObjectsWithTag("Water"), x => new WaterRegion(x.transform));
    }

    void FixedUpdate()
    {
        currentRegion = null;

        foreach (WaterRegion region in waterRegions) {
            if (region.isIn(transform.position))
                if (currentRegion is null)
                    currentRegion = region;
                else if (region.baseHeight > currentRegion.baseHeight)
                    currentRegion = region;
        }

        if (currentRegion is null) return;

        Vector3 normal = Vector3.zero;

        float height = currentRegion.Gerstner(transform.position, ref normal);
        float depth = height - transform.position.y;

        if (depth > 0) {
            depth = Mathf.Clamp01(depth);

            float volume = rb.mass / density;

            Vector3 force = normal * depth * volume * currentRegion.gravity * currentRegion.density;
            
            float vn = Vector3.Dot(rb.velocity, normal);
            Vector3 dapmingForce = -normal * vn * damping;

            Vector3 dragForce = -rb.velocity * drag;

            force = Vector3.ClampMagnitude(force, currentRegion.density * volume * currentRegion.gravity);

            rb.AddForceAtPosition(force + dapmingForce + dragForce, transform.position);
        }
    }
}
