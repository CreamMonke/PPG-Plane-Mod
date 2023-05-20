using UnityEngine;
using UnityEngine.Events;

namespace Mod
{
    public class Mod
    {
        public static void Main()
        {
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Metal Cube"),
                    NameOverride = "Propeller Plane",
                    DescriptionOverride = "A plane with a propeller.",
                    CategoryOverride = ModAPI.FindCategory("Vehicles"),
                    ThumbnailOverride = ModAPI.LoadSprite("preview.png"),
                    AfterSpawn = (Instance) =>
                    {
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("col.png");
                        Instance.gameObject.FixColliders();
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("plane.png");
                        Instance.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Bubbles");
                        Instance.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        float direction = Instance.transform.localScale.x;

                        Plane plane = Instance.AddComponent<Plane>();
                        Instance.AddComponent<UseEventTrigger>().Action = plane.ToggleOn;
                        plane.direction = direction;

                        plane.start = ModAPI.LoadSound("start.wav");
                        plane.loop = ModAPI.LoadSound("loop.wav");
                        plane.stop = ModAPI.LoadSound("stop.wav");

                        // elevator
                        Transform elevator = GameObject.Instantiate(ModAPI.FindSpawnable("Metal Cube").Prefab, Instance.transform.position + new Vector3(-3.5f * direction, 0.1f, 0f), Quaternion.identity).transform;
                        elevator.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("elevator.png");
                        elevator.gameObject.FixColliders();
                        elevator.GetComponent<Rigidbody2D>().isKinematic = true;
                        elevator.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Bubbles");
                        elevator.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        elevator.localScale = new Vector3(direction, 1f, 1f);
                        elevator.parent = Instance.transform;
                        plane.elevator = elevator;

                        // propeller blades
                        GameObject blade1 = GameObject.Instantiate(ModAPI.FindSpawnable("Knife").Prefab, Instance.transform.position + new Vector3(3.8f * direction, -0.15f, 0f), Quaternion.identity);
                        blade1.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("blade.png");
                        blade1.FixColliders();
                        blade1.GetComponent<Rigidbody2D>().isKinematic = true;
                        blade1.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Bubbles");
                        blade1.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        blade1.transform.localScale = new Vector3(direction, 1f, 1f);
                        blade1.transform.parent = Instance.transform;
                        blade1.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                        
                        GameObject blade2 = GameObject.Instantiate(ModAPI.FindSpawnable("Knife").Prefab, Instance.transform.position + new Vector3(3.8f * direction, -0.2f, 0f), Quaternion.identity);
                        blade2.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("blade.png");
                        blade2.FixColliders();
                        blade2.GetComponent<Rigidbody2D>().isKinematic = true;
                        blade2.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Bubbles");
                        blade2.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        blade2.transform.localScale = new Vector3(direction, 1f, 1f);
                        blade2.transform.parent = Instance.transform;
                        blade2.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                        
                        foreach(Collider2D c in Instance.GetComponents<Collider2D>())
                        {
                            foreach (Collider2D a in elevator.GetComponents<Collider2D>())
                            {
                                Physics2D.IgnoreCollision(a, c, true);
                            }
                            foreach (Collider2D a in blade1.GetComponents<Collider2D>())
                            {
                                Physics2D.IgnoreCollision(a, c, true);
                            }
                            foreach (Collider2D a in blade2.GetComponents<Collider2D>())
                            {
                                Physics2D.IgnoreCollision(a, c, true);
                            }
                        }

                        plane.blade1 = blade1.transform;
                        plane.blade2 = blade2.transform;

                        // engine
                        HingeJoint2D engine = GameObject.Instantiate(ModAPI.FindSpawnable("Metal Cube").Prefab, Instance.transform.position + new Vector3(3f * direction, -0.1f, 0f), Quaternion.identity).AddComponent<HingeJoint2D>();
                        engine.gameObject.layer = 10;
                        engine.transform.parent = Instance.transform;
                        engine.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("engine.png");
                        engine.gameObject.FixColliders();
                        engine.transform.localScale = new Vector3(direction, 1f, 1f);
                        engine.connectedBody = Instance.GetComponent<Rigidbody2D>();
                        JointAngleLimits2D engineLimits = new JointAngleLimits2D();
                        engineLimits.min = 0f;
                        engineLimits.max = 0f;
                        engine.limits = engineLimits;
                        engine.breakForce = 200f;
                        engine.GetComponent<Rigidbody2D>().mass = 0.01f;
                        engine.GetComponent<PhysicalBehaviour>().TrueInitialMass = 0.01f;
                        engine.GetComponent<PhysicalBehaviour>().InitialMass = 0.01f;
                        engine.gameObject.AddComponent<JointBreak>().action = () =>
                        {
                            engine.transform.parent = null;
                            plane.ToggleOn();
                            plane.on = false;
                            plane.active = false;

                            engine.GetComponent<Rigidbody2D>().mass = 1f;
                            engine.GetComponent<PhysicalBehaviour>().TrueInitialMass = 1f;
                            engine.GetComponent<PhysicalBehaviour>().InitialMass = 1f;

                            Rigidbody2D smoke = GameObject.Instantiate(ModAPI.FindSpawnable("Particle Projector").Prefab, engine.transform.position + new Vector3(0f, -0.25f, 0f), Quaternion.identity).GetComponent<Rigidbody2D>();
                            smoke.isKinematic = true;
                            GameObject.Destroy(smoke.GetComponent<Collider2D>());
                            smoke.GetComponent<ParticleMachineBehaviour>().Activated = true;
                            smoke.GetComponent<SpriteRenderer>().sprite = null;
                            smoke.transform.parent = engine.transform;
                        };

                        // wheels
                        GameObject w = ModAPI.FindSpawnable("Wheel").Prefab;
                        Vector2[] wps = { new Vector2(-3.45f, -0.4f), new Vector2(2.4f, -1.7f) };

                        for (int i = 0; i < 2; i++)
                        {
                            GameObject wheel = GameObject.Instantiate(w, Instance.transform.position + new Vector3(wps[i].x * direction, wps[i].y, 0f), Quaternion.identity);
                            wheel.transform.localScale *= i == 0 ? 0.6f : 0.8f;
                            wheel.GetComponent<SpriteRenderer>().sortingOrder = 0;
                            WheelJoint2D wj = Instance.AddComponent<WheelJoint2D>();
                            wj.connectedBody = wheel.GetComponent<Rigidbody2D>();
                            wj.anchor = wps[i];
                            wj.autoConfigureConnectedAnchor = true;
                            JointSuspension2D js = wj.suspension;
                            js.dampingRatio = 0f;
                            js.frequency = 15f;
                            wj.suspension = js;
                            if(i == 0) { plane.wheel1 = wheel.AddComponent<Wheel>(); }
                            else { plane.wheel2 = wheel.AddComponent<Wheel>(); }
                            wheel.GetComponent<Wheel>().main = Instance;
                        }

                        // menu buttons
                        UnityAction[] pitchUp = { () => { plane.pitchMode = 0; plane.pitch = 0.15f; } };
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(new ContextMenuButton("pitchUp", "Pitch Up", "The plane will ascend.", pitchUp));

                        UnityAction[] pitchDown = { () => { plane.pitchMode = 0; plane.pitch = -0.15f; } };
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(new ContextMenuButton("pitchDown", "Pitch Down", "The plane will descend.", pitchDown));
                        
                        UnityAction[] resetPitch = { () => { plane.pitchMode = 0; plane.pitch = 0f; } };
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(new ContextMenuButton("resetPitch", "Reset Pitch", "The pitch will be reset.", resetPitch));

                        UnityAction[] level = { () => { plane.pitchMode = 1; plane.pitch = 0f; } };
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(new ContextMenuButton("level", "Level", "The plane will try to keep itself level.", level));

                        UnityAction[] stabilize = { () => { plane.pitchMode = 2; plane.pitch = 0f; } };
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(new ContextMenuButton("stabilize", "Stabilize", "The plane will reduce its angular velocity.", stabilize));

                        UnityAction[] keyboard = { () => { plane.useKeyboard = !plane.useKeyboard; plane.pitch = 0f; plane.pitchMode = 0; } };
                        ContextMenuButton eK = new ContextMenuButton("keyboard", "Disable Keyboard Control", "Toggle if the plane can be controled with IJKL. I/K - throttle ; J/L - pitch", keyboard);
                        eK.LabelGetter = () => plane.useKeyboard ? "Disable Keyboard Control" : "Enable Keyboard Control";
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(eK);
                        
                        UnityAction[] brake = { () => { plane.wheel1.ToggleBrake(); plane.wheel2.ToggleBrake(); } };
                        ContextMenuButton bB = new ContextMenuButton("brake", "Disable Wheel Brakes", "The wheel brakes will be toggled on/off. This is also done automatically when the plane is turned on/off.", brake);
                        bB.LabelGetter = () => plane.wheel1.braked ? "Disable Wheel Brakes" : "Enable Wheel Brakes";
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(bB);

                        UnityAction[] explosion = { () => { plane.canExplode = !plane.canExplode; } };
                        ContextMenuButton eB = new ContextMenuButton("explosion", "Disable Explosion", "Sets if the plane can explode or not. An explosion will only occur if the plane is running.", explosion);
                        eB.LabelGetter = () => plane.canExplode ? "Disable Explosion" : "Enable Explosion";
                        Instance.GetComponent<ContextMenuOptionComponent>().Buttons.Add(eB);
                    }
                }
            );
        }
    }

    public class Plane : MonoBehaviour
    {
        public Transform blade1;
        public Transform blade2;
        public Wheel wheel1;
        public Wheel wheel2;
        public CarBehaviour car;
        public Transform elevator;
        public AudioClip loop;
        public AudioClip start;
        public AudioClip stop;
        private AudioSource source;
        private Rigidbody2D rb;
        private PhysicalBehaviour pb;
        private ExplosionCreator.ExplosionParameters ep;

        private float thrust = 0f;
        private const float forceCoefficient = 0.001f;
        private const float maxThrust = 600f;
        private const float maxVelocity = 80f;
        private const float maxAngularV = 40f;

        public float direction = 1f; // either 1 or -1
        public int pitchMode = 0; // 0 - up/down 1 - stabilize 2 - maintain pitch
        public float pitch = 0f;
        public float throttle = 1f; // this is used during WASD control
        public bool active = true;
        public bool on = false;
        public bool canExplode = true;
        public bool useKeyboard = false;
        private float t = 0f;
        private float bladeSpeed = 0f;
        private float audioTime = 0f;
        private bool startAudio = true;

        public void ToggleOn() 
        {
            if(!active){ return; }
            on = !on;
            thrust = 0f;
            if(on) { throttle = 1f; }
            rb.gravityScale = 1f;
            wheel1.UpdateMass();
            wheel2.UpdateMass();
            wheel1.ToggleBrake();
            wheel2.ToggleBrake();
        }
        void Start()
        { 
            rb = GetComponent<Rigidbody2D>();
            pb = GetComponent<PhysicalBehaviour>();

            rb.drag = 0.1f;

            rb.mass = 100f;
            pb.TrueInitialMass = 100f;
            pb.InitialMass = 100f;

            // initialize constant parameters
            ep = new ExplosionCreator.ExplosionParameters(20, Vector3.zero, 30f, 30f, true, ExplosionCreator.EffectSize.Large, 0.75f, 20);

            source = gameObject.AddComponent<AudioSource>();
            gameObject.AddComponent<AudioSourceTimeScaleBehaviour>(); // this will distort the audio if the game is in slomo or paused
        }
        /* 
        Graphs:
        
        Lift: https://www.desmos.com/calculator/nzyzsv7rut
        Thrust Gain: https://www.desmos.com/calculator/dnk3qhbzub
        Drift Removal Rate: https://www.desmos.com/calculator/lvdikwv1sn
        Wheel Mass: https://www.desmos.com/calculator/f32lm5qeko
        Propeller Blade Animation: https://www.desmos.com/calculator/ommxgpmesu
        */
        void FixedUpdate()
        {
            float dot = -transform.right.y * direction; // this is the same as Vector2.Dot(-Vector2.up, transform.right); or (0 * x) + (-1 * y)

            float percentOfMaxSpeed = (rb.velocity.magnitude / maxVelocity);
            percentOfMaxSpeed = percentOfMaxSpeed < 0 ? 0 : percentOfMaxSpeed > 1 ? 1 : percentOfMaxSpeed;
            rb.gravityScale = (-0.9f * percentOfMaxSpeed + 1); // simulate lift by scaling gravity with the percent of max speed

            if (pitchMode == 1)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, 0.025f * (1f - Mathf.Abs(dot)));
                pitch = 0.2f * (dot + 0.025f); // move towards level. adding to the dot so the plane will tilt up a little
            }
            else if (pitchMode == 2) { rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, 0.02f); } // slowly move to 0 velocity

            rb.angularVelocity += pitch * direction; // add angular force

            if(!on){return;}

            float av = rb.angularVelocity;
            rb.angularVelocity = av > maxAngularV ? maxAngularV : av < -maxAngularV ? -maxAngularV : av; // cap the angular velocity

            if(throttle == 0) { thrust = 0; }
            else
            {
                float thrustGain = -0.375f * (dot*dot) + 1.375f * dot + 1; // calculate the thrust to add
                thrust += thrustGain;
                thrust = thrust > maxThrust ? maxThrust : thrust < 0 ? 0 : thrust; // make sure its not above the max or below 0
            }

            float currentMaxV = throttle * maxVelocity * (0.2f * dot + 1); // calculate the max velocity based on the dot
            if(rb.velocity.magnitude < currentMaxV) { rb.velocity += forceCoefficient * thrust * (Vector2)transform.right * direction; } // accelerate in the direction of the plane

            if (thrust > 300) { rb.velocity = rb.velocity.magnitude * Vector2.Lerp(rb.velocity.normalized, transform.right * direction, 0.2f * dot * dot); } // remove drift by transitioning the velocity direction to the plane's direction over time

            float newMass = -0.06f * rb.velocity.magnitude + 5f; // for the wheels; their mass is less if the speed is higher
            newMass = newMass < 0 ? 0 : newMass;
            wheel1.UpdateMass(newMass);
            wheel2.UpdateMass(newMass);
        }
        void Update()
        {
            if(active && (pb.IsUnderWater || pb.IsInLava)) // the plane will stop working if it goes under water or lava
            {
                ToggleOn();
                on = false;
                active = false;
                pitchMode = 0;
            }

            // keyboard control
            if(useKeyboard)
            {
                if(Input.GetKey(KeyCode.J)) { pitch = 0.25f; }
                else if(Input.GetKey(KeyCode.L)) { pitch = -0.25f; }
                else 
                { 
                    pitch = 0;
                    rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, Time.deltaTime*2f);
                }

                if (throttle < 1f && Input.GetKey(KeyCode.I)) { throttle += 0.0025f; }
                else if (throttle > 0f && Input.GetKey(KeyCode.K)) { throttle -= 0.0025f; }
                throttle = throttle < 0f ? 0f : throttle > 1f ? 1f : throttle;
            }
            
            // propeller
            if(thrust > 0f && bladeSpeed < 100f){ bladeSpeed += Time.deltaTime * 10f * throttle; }
            else if(thrust == 0f) { bladeSpeed -= Time.deltaTime * 10f; }
            bladeSpeed = bladeSpeed < 0f ? 0f : bladeSpeed;
            t += Time.deltaTime * bladeSpeed;
            float scale =  0.5f * Mathf.Cos(t) + 0.5f;
            blade1.localScale = new Vector3(scale, 1f, 1f);
            blade2.localScale = new Vector3(scale, 1f, 1f);
            
            // elevator
            float eleTarget = pitch * 125f;
            if (pitchMode == 1) { eleTarget *= 4f; } // make it look more dramatic so its visible
            else if (pitchMode == 2) { eleTarget = -rb.angularVelocity * direction * 2f; }
            eleTarget = eleTarget < 0 ? -eleTarget : 360 - eleTarget;
            eleTarget = eleTarget > 30 && eleTarget < 180 ? 30 : eleTarget < 330 && eleTarget > 180 ? 330 : eleTarget; // clamp the angle 
            elevator.localRotation = Quaternion.RotateTowards(elevator.localRotation, Quaternion.Euler(0, 0, eleTarget), 100f * Time.deltaTime);

            // audio
            if(thrust > 0f)
            { 
                audioTime += Time.deltaTime;
                if (startAudio)
                {
                    source.loop = false;
                    source.clip = start;
                    source.Play();
                    startAudio = false;
                }
                if (!source.loop && audioTime > 10f)
                {
                    source.clip = loop;
                    source.loop = true;
                    source.Play();
                }
            }
            else if(!startAudio)
            {
                source.loop = false;
                source.clip = stop;
                source.Play();
                startAudio = true;
                audioTime = 0f;
            }
        }
        void OnCollisionEnter2D(Collision2D e)
        {
            if(!active || !on || !canExplode || throttle < 0.8f){ return; }
            float force = rb.velocity.magnitude * (e.rigidbody.mass > 100 ? 100 : e.rigidbody.mass); // the mass is capped at 100 because of the boundries
            if(force > 1000f)
            {
                for(int i = 0; i < 5; i++)
                {
                    ep.Position = transform.position + new Vector3(1.5f * i - 3, 0f, 0f);
                    ExplosionCreator.CreateFragmentationExplosion(ep);
                }

                GameObject.Destroy(gameObject);
            }
        }
        void OnDestroy()
        {
            GameObject.Destroy(wheel1.gameObject);
            GameObject.Destroy(wheel2.gameObject);
        }
    }
    public class Wheel : MonoBehaviour // this just controls the mass
    {
        public bool braked = true;
        
        public GameObject main; // this is so if the wheels get deleted, the plane will be too. the reason it isn't parented is so that the plane wont explode when it registers a hit on the wheels
        private Rigidbody2D rb;
        private PhysicalBehaviour pb;
        void Start()
        { 
            rb=GetComponent<Rigidbody2D>();
            pb=GetComponent<PhysicalBehaviour>();
            UpdateMass();
            rb.angularDrag = 500f;
        }
        void OnDestroy()
        {
            GameObject.Destroy(main);
        }
        public void UpdateMass(float newMass=5f)
        {
            rb.mass = newMass;
            pb.TrueInitialMass = newMass;
            pb.InitialMass = newMass;
        }
        public void ToggleBrake()
        {
            rb.angularDrag = rb.angularDrag == 0.05f ? 500f : 0.05f;
            braked = !braked;
        }
    }
    public class JointBreak : MonoBehaviour
    {
        public delegate void Del();
        public Del action;
        void OnJointBreak2D(Joint2D brokenJoint) { action(); }
    }
}