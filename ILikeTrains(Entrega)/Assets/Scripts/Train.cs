using UnityEngine;
using System.Collections;

/// <summary>
/// Tren: Cada tren tiene un array de horas y minutos para el día y otro para la noche, de manera que pueden salir más veces
/// de manera diurna que nocturna si eso es lo que se desea. Además, puede tener 2 rutas si se desea. En caso de que solo 
/// tenga una, el segundo array de puntos quedará vacío. Estos puntos de ruta, se establecen desde el inspector. El 
/// movimiento se calcula via NavMesh de Unity.
/// </summary>
public class Train : MonoBehaviour
{
    /// <summary>
    /// Variables
    /// </summary>
    #region variables

    //Variables
    [SerializeField] private AudioClip[] m_Audios;         // Audios del tren
    [SerializeField] private GameObject[] m_WaypointsR1;   // Puntos de la ruta 1
    [SerializeField] private GameObject[] m_WaypointsR2;   // Puntos de la ruta 2. Si no se ponen, solo tendrá una ruta
    [SerializeField] private Light m_TrainLight;           // Luz del tren que se enciende cuando se hace de noche.
    [SerializeField] private int[] m_DayHour;              // Horas de salida de los trenes por el día
    [SerializeField] private int[] m_DayMinute;            // Minutos de las horas de salida de los trenes por el día
    [SerializeField] private int[] m_NightHour;            // Horas de salida de los trenes por la noche
    [SerializeField] private int[] m_NightMinute;          // Minutos de las horas de salida de los trenes por el día
    [SerializeField] private float m_MovementMaxSpeed;     // Máxima velocidad a la que se puede mover el tren. El mínimo es cero. 
    [SerializeField] private float m_MovementSpeed;        // Velocidad actual a la que se mueve el tren.
    [SerializeField] private bool m_CanMove;               // Si el tren puede moverse o no. Si se puede mover, se mueve, sino no.
    [SerializeField] private int m_ID;                     // Id de cada tren
    [SerializeField] private int m_Route;                  // Ruta que sigue el tren

    private AudioSource m_adAudioSource;                   // AudioSource
    private GameObject[] m_Waypoints;                      // Array de Waypoints que está siguiendo actualmente
    private NavMeshAgent m_nmaNavMeshAgent;                // Agente NavMesh de los trenes que permite moverlo por las vías.
    private Transform m_tMyTransform;                      // Componente transform del tren
    private Vector3 m_v3InitialPosition;                   // Posición inicial del tren. Se restaurará al llegar al destino.
    private Quaternion m_v3InitialRotation;                // Rotación inicial del tren. Se restaurará al llegar al destino.
    private int m_iActualWayPoint = 0;                     // Punto de la ruta por el que va actualmente
    private bool m_bActiveTrain = false;                   // Si el tren ha salido ya o no de la estación.


    //Properties
    public Light Light { get { return m_TrainLight; } }
    public bool CanMove { set { m_CanMove = value; } get { return m_CanMove; } }
    public bool isActive { set { m_bActiveTrain = value; } get { return m_bActiveTrain; } }
    public int ID { set { m_ID = value; } get { return m_ID; } }
    public int[] StartingHourDay { get { return m_DayHour; } }
    public int[] StartingMinutesDay { get { return m_DayMinute; } }
    public int[] StartingHourNight { get { return m_NightHour; } }
    public int[] StartingMinutesNight { get { return m_NightMinute; } }
    #endregion

    // Use this for initialization
    void Start()
    {
        m_MovementSpeed = m_MovementMaxSpeed;
        m_nmaNavMeshAgent = GetComponent<NavMeshAgent>();
        m_nmaNavMeshAgent.updatePosition = true;
        m_tMyTransform = GetComponent<Transform>();
        m_adAudioSource = GetComponent<AudioSource>();
        m_v3InitialPosition = m_tMyTransform.position;
        m_v3InitialRotation = m_tMyTransform.rotation;
        SetNewRandomRoute();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_bActiveTrain && m_CanMove)
        {
            
            if (m_Waypoints.Length == 0)
            {
                Debug.LogError("El tren con ID: " + ID + " no puede salir porque no se le han puesto waypoints");
                return;
            }

            //Debug.Log("V:" + Vector3.Distance(m_tMyTransform.position, m_Waypoints[m_iActualWayPoint].transform.position));
            Debug.DrawRay(transform.position, transform.forward * 20, Color.green);
            Debug.DrawRay(transform.position, (transform.forward + transform.right).normalized * 20, Color.green);
            Debug.DrawRay(transform.position, (transform.forward - transform.right).normalized * 20, Color.green);

            if (Vector3.Distance(m_tMyTransform.position, m_Waypoints[m_iActualWayPoint].transform.position) < m_nmaNavMeshAgent.speed)
            {
                m_iActualWayPoint++;

                if (m_iActualWayPoint >= m_Waypoints.Length)
                {
                    StopTrainInStation();
                    return;
                }

                // Set the agent to go to the currently selected destination.
                m_nmaNavMeshAgent.SetDestination(m_Waypoints[m_iActualWayPoint].transform.position);
            }
        }
    }

    //=================================================================================================================================//
    //                  ** Método que establece la velocidad del NavMeshAgent a la que ponemos en el tren**                            //
    //=================================================================================================================================//

    /// <summary>
    /// Método que pone la velocidad del agente a la que ponemos en el tren desde el inspector
    /// </summary>
    public void SetAgentSpeed()
    {
        m_nmaNavMeshAgent.speed = m_MovementSpeed;//GameManager.Instance.TimeSpeed * m_MovementSpeed;
    }

    //=================================================================================================================================//
    //                  ** Método que inicializa la dirección del tren al primer waypoint de la ruta.**                                //
    //=================================================================================================================================//

    /// <summary>
    /// Método que pone el tren en dirección al primer waypoint de la ruta
    /// </summary>
    public void SetAgentZero()
    {
        m_adAudioSource.PlayOneShot(m_Audios[0], 0.5f);
        m_iActualWayPoint = 0;
        m_nmaNavMeshAgent.SetDestination(m_Waypoints[m_iActualWayPoint].transform.position);
        Debug.Log("El tren con ID:" + ID + " sale de la estación de origen.");
    }

    //=================================================================================================================================//
    //                  ** Método que para el tren. Esto se llama cuando llega al último waypoint: Al final de la ruta.**              //
    //=================================================================================================================================//

    /// <summary>
    /// Método que para el tren al llegar al final de la ruta
    /// </summary>
    public void StopTrainInStation()
    {
        m_adAudioSource.PlayOneShot(m_Audios[0], 0.5f);
        m_iActualWayPoint = 0;
        Debug.Log("El tren con ID:" + ID + " ha llegado a la estación de destino.");
        m_nmaNavMeshAgent.speed = 0;                    // La nueva velocidad es cero
        m_nmaNavMeshAgent.Stop();                       // Paramos el navmeshagent
        m_bActiveTrain = false;                         // Desactivamos el tren, pues ahora está en la estación
        m_CanMove = false;                              // No se puede mover
        GameManager.Instance.DisableActualCamera();     // Desactivamos la cámara actual (Se pone la cero)
        ResetPosition();                                // Reseteamos la posición a la inicial
        m_TrainLight.intensity = 0;                     // Quitamos la luz (Focos del tren)
    }

    //=================================================================================================================================//
    //                  ** Método que para el tren. Se llama cuando llega a un semáforo en el que tiene que pararse.**                 //
    //=================================================================================================================================//

    /// <summary>
    /// Método que para el tren.
    /// </summary>
    /// 
    public void StopTrain()
    {
        m_bActiveTrain = false;
        m_CanMove = false;
        m_nmaNavMeshAgent.Stop();
        Debug.Log("Paramos el tren con ID" + ID);
    }

    //=================================================================================================================================//
    //                  ** Método que Arranca el tren y le hace continuar por donde iba.**                                             //
    //=================================================================================================================================//

    /// <summary>
    /// Método que se llama desde TrainLight una vez que el otro tren le permite ya pasar. En vez de establecerlo de 
    /// cero, lo hace continuar por donde iba.
    /// </summary>
    public void BootTrain()
    {
        m_adAudioSource.PlayOneShot(m_Audios[0], 0.5f);
        m_bActiveTrain = true;
        m_CanMove = true;
        m_nmaNavMeshAgent.Resume();
        Debug.Log("Arrancamos el tren con ID" + ID);
    }

    //=================================================================================================================================//
    //                  ** Método que resetea la posición y rotación a la inicial**                                                    //
    //=================================================================================================================================//

    /// <summary>
    /// Reseteamos la posición y la rotacion a la que guardamos inicialmente
    /// </summary>
    public void ResetPosition()
    {
        m_tMyTransform.position = m_v3InitialPosition;
        m_tMyTransform.rotation = m_v3InitialRotation;
        m_nmaNavMeshAgent.Warp(m_tMyTransform.position);
    }

    //=================================================================================================================================//
    //                  ** Método que establece una ruta aleatoria. Para que un tren pueda tomar una ruta alternativa si puede.**      //
    //=================================================================================================================================//

    /// <summary>
    /// En caso de tener una segunda ruta, se puede dar la posibilidad de tomarla en vez de la ruta normal.
    /// </summary>
    public void SetNewRandomRoute()
    {
        if (m_WaypointsR2.Length > 0)
            m_Route = (int)Random.Range(0f, 1.9f);
        else
            m_Route = 0;

        switch (m_Route)
        {
            case 0:
                m_Waypoints = m_WaypointsR1;
                break;
            case 1:
                m_Waypoints = m_WaypointsR2;
                break;
        }
    }
}
