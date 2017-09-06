using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// GameManager: Se encarga de controlar el juego en sí. Tiene una sola instancia del mismo en la escena.
/// Controla el "tiempo". Los trenes salen a determinadas horas. Este script controla el tiempo del mundo.
/// Cuando llega la hora a la que se hace de noche, el skybox se pone en noche. También pasa con el día.
/// </summary>

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Variables
    /// </summary>
    #region variables
    //Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    [SerializeField] private TrainManager m_tTrainManagerReference;     // Referencia al Trainer Manager por si se necesita
    [SerializeField] private int m_DayTimeHour;                         // Contador de horas de la Hora de juego.
    [SerializeField] private int m_DayTimeMinutes;                      // Contador de horas de minutos de juego.
    [SerializeField] private int m_TimeToBeNight;                       // Hora a la que se hace de noche. Por defecto son las 18:00
    [SerializeField] private int m_TimeToBeDay;                         // Hora a la que se hace de día. Por defecto son las 09:00
    [SerializeField] private float m_DayTime;                           // Contador de delta time
    [SerializeField] private float m_MaxTimeSpeed;                      // Velocidad a la que llega el tiempo en modo acelerado
    [SerializeField] private float m_MinTimeSpeed;                      // Velocidad a la que llega el tiempo en modo normal
    [SerializeField] private float m_TimeSpeed;                         // Velocidad a la que va el tiempo.
    [SerializeField] private bool m_TwelveHoursMode;                    // Modo de doce horas. En falso, utiliza un modo de 24 horas.
    [SerializeField] private bool m_WorldStopped;                       // Cuando pulsas un botón (Q), todo se pause.
    [SerializeField] private Camera[] m_Cameras;                        // Cámaras de los trenes
    [SerializeField] private Camera m_cActualCamera;                    // Cámara que está renderizando actualmente
    [SerializeField] private Material[] m_SkyBoxes;                     // Skyboxes: Skybox0=Día, Skybox1=Noche
    [SerializeField] private Material m_skActualSkybox;                 // Skybox activo  
    [SerializeField] private Text m_timeText;                           // Texto que muestra en pantalla la hora que es. Es falsa. No corresponde a la real, ya que sino, sería muy lento.

    private bool m_bIsDay;                                              // Comprobador de si es día. Sino, es noche. 
    private AudioSource m_AudioSource;                                  // Audiosource

    //Properties
    public TrainManager TrainManager { get { return m_tTrainManagerReference; } }
    public float Time { get { return m_DayTime; } set { m_DayTime = value; } }
    public int Hour { get { return m_DayTimeHour; } }
    public int Minute { get { return m_DayTimeMinutes; } }
    public float TimeSpeed { get { return m_TimeSpeed; } set { m_TimeSpeed = value; } }
    public bool TwelveTimeMode { get { return m_TwelveHoursMode; } }
    public bool IsDay { get { return m_bIsDay; } }
    #endregion


    //=================================================================================================================================//
    //                  ** Método que se ejecuta antes que nada e inicializa la instancia del singleton si es necesario **             //
    //=================================================================================================================================//
    /// <summary>
    /// Awake
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameSetUp();
            DontDestroyOnLoad(this);
        }
        else
        {
            Debug.LogError("[ErrorLog en GameManager.cs]: Ya hay una instancia previa de GameManager");
        }
    }

    //=================================================================================================================================//
    //                  ** Método que se al iniciar el programa. Establece el modo 12H/24H según esté en el inspector **               //
    //=================================================================================================================================//

    /// <summary>
    /// Start
    /// </summary>
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_AudioSource.Play();               //Reproducimos la música de fondo

        if (m_TwelveHoursMode)
        {
            ActivateTwelveHourMode(true);   //Si en el inspector hemos puesto el modo 12 H, lo activamos
        }
        else
        {
            ActivateTwelveHourMode(false);  //Si en el inspector no hemos puesto el modo 12 H, ponemos modo 24H
        }
    }

    //=================================================================================================================================//
    //                  ** Método que establece lo necesario inicialmente: Skybox, Cámaras, Hora, Tiempo... **                         //
    //=================================================================================================================================//

    /// <summary>
    ///  Game Set Up
    /// </summary>
    public void GameSetUp()
    {
        DisableAllCameras();                            // Desactivamos todas las cámaras menos la 0: La general
        RenderSettings.skybox = m_skActualSkybox;       // Establecemos el skybox al que queramos inicialmente. Después se cambia según la hora que hayamos puesto.
        SetTimeText(0f);                                // Ponemos el tiempo en el texto que se ve por pantalla.
        m_TimeSpeed = m_MinTimeSpeed;                   // Establecemos la velocidad a la que va el tiempo
    }

    //=================================================================================================================================//
    //                  ** Método que se llama desde un botón del canvas. Acelera la velocidad a la que pasa el tiempo.**              //
    //=================================================================================================================================//

    /// <summary>
    ///  Método Activar el modo acelerado. En un principio se debería acelerar todo.
    /// </summary>
    /// <param name="activation">Si activamos o no el modo acelerado</param>
    public void ActivateAcceleratedMode(bool activation)
    {
        if (!activation)
        {
            //m_TimeSpeed = m_MinTimeSpeed;     // El tiempo a velocidad normal
            m_tTrainManagerReference.AccelerateWorld(m_MinTimeSpeed / 10);
        }
        else
        {
            //m_TimeSpeed = m_MaxTimeSpeed;     // El tiempo a velocidad acelerada
            m_tTrainManagerReference.AccelerateWorld(m_MaxTimeSpeed / 50);
        }
       
    }

    //=================================================================================================================================//
    //                  ** Método que se llama desde el Start(). Activa y desactiva el modo 12H. Mejor ponerlo en modo 24H.**          //
    //=================================================================================================================================//

    /// <summary>
    /// Método Activador del modo 12H. 
    /// </summary>
    /// <param name="activation">True: Modo 12H, false: Modo 24H</param>
    public void ActivateTwelveHourMode(bool activation)
    {
        m_TwelveHoursMode = activation;

        if (m_TwelveHoursMode)
        {
            if (m_TimeToBeDay > 12)
            {
                m_TimeToBeDay -= 12;

                if (m_TimeToBeDay > 24)
                {
                    m_TimeToBeDay = 9;
                }

                if (m_TimeToBeNight > 24)
                {
                    m_TimeToBeDay = 6;
                }
            }

            if (m_TimeToBeNight > 12)
            {
                m_TimeToBeNight -= 12;
            }
        }
        else
        {
            if (m_TimeToBeDay > 24)
            {
                m_TimeToBeDay = 9;
            }

            if (m_TimeToBeNight > 24)
            {
                m_TimeToBeDay = 18;
            }
        }

       //Establecemos el Skybox según la hora que sea
       if (m_DayTimeHour >= m_TimeToBeDay && m_DayTimeHour < m_TimeToBeNight)
        {
            m_bIsDay = true;
            if (m_skActualSkybox != m_SkyBoxes[0]) // Si no es día ya (es noche), lo ponemos a día.
                SetSkybox(m_bIsDay);
            Debug.Log("Is Day");
        }
       else
        {
            m_bIsDay = false;
            if (m_skActualSkybox != m_SkyBoxes[1]) // Si no es noche ya (es día), lo ponemos a noche.
                SetSkybox(m_bIsDay);
            Debug.Log("Is Night");
        }
    }

    //=================================================================================================================================//
    //                  ** Método que cambia el skybox según sea de día o noche.**                                                     //
    //=================================================================================================================================//

    /// <summary>
    /// Método que establece el skybox si es día o noche
    /// </summary>
    /// <param name="day"> Si es día o noche. True: Día, False: Noche</param>
    public void SetSkybox(bool day)
    {
        if (day)
        {
            ChangeSkyboxMaterial(0); // Ponemos un nuevo material al skybox: día
        }
        else
        {
            ChangeSkyboxMaterial(1); // Ponemos un nuevo material al skybox: noche
        }
    }

    //=================================================================================================================================//
    //                  ** Método que modifica el texto de la hora que se ve en pantalla.**                                            //
    //=================================================================================================================================//

    /// <summary>
    /// Método que establece el tiempo que se ve en pantalla
    /// </summary>
    /// <param name="deltaTime"> Recibe un Time.deltaTime.</param>
    public void SetTimeText(float deltaTime)
    {
        m_DayTime += deltaTime * m_TimeSpeed;

        m_DayTimeMinutes = (int)m_DayTime / 60;
        
        if (m_TwelveHoursMode)
        {
            if (m_DayTimeHour > 12)
            {
                m_DayTimeHour -= 12;
            }
        }
        else
        {
            if (m_DayTimeHour > 24)
            {
                m_DayTimeHour -= 24;
            }
        }

        if (m_DayTimeMinutes>=60)
        {
            m_DayTime = 0;
            m_DayTimeMinutes = 0;
            m_DayTimeHour++;

            if (m_DayTimeHour == m_TimeToBeDay)
            {
                m_bIsDay = true;
                SetSkybox(m_bIsDay);
            }
            else if (m_DayTimeHour == m_TimeToBeNight)
            {
                m_bIsDay = false;
                SetSkybox(m_bIsDay);
            }
        }

        if (m_DayTimeHour < 10)
        {
            if (m_DayTimeMinutes < 10)
            {
                m_timeText.text = "0" + m_DayTimeHour.ToString() + ":" + "0" + m_DayTimeMinutes.ToString();
            }
            else
            {
                m_timeText.text = "0" + m_DayTimeHour.ToString() + ":" + m_DayTimeMinutes.ToString();
            }
        }
        else
        {
            if (m_DayTimeMinutes < 10)
            {
                m_timeText.text = m_DayTimeHour.ToString() + ":" + "0" + m_DayTimeMinutes.ToString();
            }
            else
            {
                m_timeText.text = m_DayTimeHour.ToString() + ":" + m_DayTimeMinutes.ToString();
            }
        }
    }

    //=================================================================================================================================//
    //                  ** Desactivamos todas las cámaras excepto la actual.**                                                         //
    //=================================================================================================================================//

    /// <summary>
    ///  Desactivador de las cámaras de la escena
    /// </summary>
    /// <param name="camerazero">True: La cámara 0 es la actual y desactiva el resto. False: Desactiva todas menos la actual.</param>
    public void DisableAllCameras(bool camerazero=false)
    {
        if (camerazero && m_cActualCamera != m_Cameras[0])
        {
            m_cActualCamera = m_Cameras[0];
            m_cActualCamera.enabled = true;
        }

        for (int i = 0; i < m_Cameras.Length; i++)
        {
            if (m_Cameras[i] != m_cActualCamera)
            {
                m_Cameras[i].enabled = false;
            }
        }
    }

    //=================================================================================================================================//
    //                  ** Método que desactiva la cámara actual y ponemos la cero**                                                   //
    //=================================================================================================================================//

    /// <summary>
    /// Desactiva la cámara actual y pone la cero. Se usa cuando el tren llega a su destino, si es que la suya se está usando.
    /// </summary>
    public void DisableActualCamera()
    {
        if (m_cActualCamera != m_Cameras[0])
        {
            if (!m_Cameras[0].enabled)
                m_Cameras[0].enabled = true;

            m_cActualCamera.enabled = false;
            m_cActualCamera = m_Cameras[0];
        }
       
    }

    //=================================================================================================================================//
    //                  ** Método que cambia el skybox, en caso de que tengamos varios**                                               //
    //=================================================================================================================================//

    /// <summary>
    /// Skybox al que cambiamos. Para probarlo, si desde el inspector ponemos la hora a 18-24, en modo 24H, se pone de noche
    /// </summary>
    /// <param name="i"> Posición del array que tiene los materiales de los skybox. 0: Día, 1:Noche</param>
    public void ChangeSkyboxMaterial (int i)
    {
        if (i < 0 || i >= m_SkyBoxes.Length || m_SkyBoxes[i] == null)
        {
            Debug.LogError("No existe ese skybox (Posición:"+i+")");
            return;
        }

        if (m_SkyBoxes[i] == m_skActualSkybox)
        {
            Debug.Log("Ese skybox ya está activo");
            return;
        }

        m_skActualSkybox = m_SkyBoxes[i];               // Ponemos el skybox según toque en el actual
        RenderSettings.skybox = m_skActualSkybox;       // Lo hacemos visible en escena
    }

    //=================================================================================================================================//
    //                               ** Método que cambia la cámara a la del tren que queramos **                                      //
    //=================================================================================================================================//

    /// <summary>
    /// Método que cambia la cámara a la que le digamos. Puedes cambiar con F1,F2, F3 y F4 entre las cámaras de los trenes y la general
    /// </summary>
    /// <param name="i"> Posición del array de cámaras</param>
    public void ChangeCamera (int i)
    {

        if (i < 0 || i >= m_Cameras.Length || m_Cameras[i] == null)
        {
            Debug.LogError("No existe esa cámara");
            return;
        }
        else if (m_cActualCamera == m_Cameras[i])
        {
            Debug.Log("Esa cámara ya está seleccionada");
            return;
        }
        else if (i > 0 && !m_tTrainManagerReference.Trains[i-1].isActive)
        {
            Debug.Log("Ese tren todavía no ha salido de la estación.");
            return;
        }

        m_Cameras[i].enabled = true;        // Activamos la nueva cámara actual
        m_cActualCamera.enabled = false;    // Desactivamos la cámara actual
        m_cActualCamera = m_Cameras[i];     // Cambiamos la cámara actual 
    }

    //=================================================================================================================================//
    //                               ** Método que para el Time.timeScale **                                                           //
    //=================================================================================================================================//
    /// <summary>
    /// Método que para todo. Se llama desde un botón del canvas
    /// </summary>
    public void StopEverything()
    {
        m_WorldStopped = !m_WorldStopped;

        if (m_WorldStopped)
            m_tTrainManagerReference.StopEverything(true); //No sé por qué, no coge la referencia a la librería Time, por lo que se lo pasamos a TrainManager
        else
            m_tTrainManagerReference.StopEverything(false);
    }
}
