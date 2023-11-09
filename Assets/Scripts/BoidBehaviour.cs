using UnityEngine;
using System.Collections;

public class BoidBehaviour : OrbitThing
{

    [Range(0.1f, 200.0f)]
    float boid_velocity = 6.0f;

    [Range(0.0f, 0.9f)]
    float boid_velocityVariation = 10.5f;

    [Range(0.1f, 20.0f)]
    float boid_rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)]
    float boid_neighborDist = 2.0f;

    // every second, we're going to figure out where the boids are w/ relation to each other
    // and if they're far away, they'll just sit there. If they're close to the ship, they'll converge

    // Reference to the controller.
    public GameControllerScript controller;

    // Options for animation playback.
    public float animationSpeedVariation = 0.2f;

    GameObject m_pShip;

    // Random seed.
    float noiseOffset;

    float AngleBetweenBoidAndShip( )
    {
        // find the angle between the ship vector and the boid
        return Vector3.Angle( m_pShip.transform.position, transform.position );
    }

    // calculates the separation vector with a target.
    Vector3 GetSeparationVector(Transform target)
    {
        var diff = transform.position - target.transform.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / boid_neighborDist);
        return diff * (scaler / diffLen);
    }

    internal override void Start()
    {
        base.Start();

        controller = GameControllerScript.Singleton;
        m_pShip = ShipScript.Singleton.gameObject;

        noiseOffset = Random.value * 10.0f;
    }

    void Update()
    {
        OrbitThing ot = gameObject.GetComponent<OrbitThing>();
        if( !ot.InOrbit)
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        // Current velocity randomized with noise.
        float noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2.0f - 1.0f;
        float velocity = boid_velocity * (1.0f + noise * boid_velocityVariation);

        // Initializes the vectors.
        Vector3 separation = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        // Looks up nearby boids. Boids near this boid will be planar "enough" so the
        // fact they're wrapped on a planet's surface doesn't matter
        //
        Collider [] nearbyBoids = Physics.OverlapSphere(currentPosition, boid_neighborDist, LayerMask.NameToLayer( "BlueBugs" ) );

        // Accumulates the vectors.
        foreach (Collider boidCollider in nearbyBoids)
        {
            if (boidCollider.gameObject == gameObject)
                continue;
            Transform t = boidCollider.transform;
            separation += GetSeparationVector(t);
            cohesion += t.position;
        }

        int groupSize = nearbyBoids.Length;
        float avg = 1.0f / groupSize;
        cohesion /= groupSize;
        cohesion = (cohesion - currentPosition).normalized;

        // goal seeking
        Vector3 vGoal = m_pShip.transform.position - transform.position;

        // Calculates a rotation from the vectors.
        Vector3 vMoveDirection = separation + cohesion + vGoal * 10.5f;
        Quaternion qNewRotation = Quaternion.FromToRotation(Vector3.forward, vMoveDirection.normalized);

        // Applys the rotation with interpolation.
        if (qNewRotation != currentRotation)
        {
            float ip = Mathf.Exp(-boid_rotationCoeff * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(qNewRotation, currentRotation, ip);

        }

        Vector3 vForceDirection = transform.forward * ( velocity * Time.deltaTime );
        Rigidbody rb = GetComponent<Rigidbody>( );
        rb.AddForce( vForceDirection );
    }
}
