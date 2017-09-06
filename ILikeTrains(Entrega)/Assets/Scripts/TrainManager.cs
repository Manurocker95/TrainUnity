using UnityEngine;
using System.Collections;


/// <summary>
/// Controlador de trenes. Básicamente, tiene un array de trenes y los hace arrancar cuando es su hora.
/// </summary>
public class TrainManager : MonoBehaviour
{
    /// <summary>
    /// Variables. Se había pensado con singleton, por si se llama desde algún lado que no sea el GameManager, que tiene ya su referencia.
    /// </summary>
    #region variables
    //Singleton
    private static TrainManager instance;
    public static TrainManager Instance { get { return instance; } }

    //Otras variables
    [SerializeField] private Train[] m_TrainList;       // Lista de trenes
   
    //properties
    public Train[] Trains { get { return m_TrainList; } }

    #endregion

    //=================================================================================================================================//
    //                  ** Método que se ejecuta antes que nada e inicializa la instancia del singleton si es necesario **             //
    //=================================================================================================================================//
    /// <summary>
    /// Awake: Método que se ejecuta antes que nada e inicializa la instancia del singleton si es necesario
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("[ErrorLog en GameManager.cs]: Ya hay una instancia previa de TrainManager");
        }
    }

    //=================================================================================================================================//
    //                  ** Método que se ejecuta cada frame. Las acciones ocurren aquí: Cambios de cámara... etc **                    //
    //=================================================================================================================================//

    /// <summary>
    /// Update
    /// </summary>
    void Update()
    {
        //Le decimos al gamemanager que ponga el texto en hora :D
        GameManager.Instance.SetTimeText(Time.deltaTime);

        //Si es día
        if (GameManager.Instance.IsDay)
        {
            //Usamos los horarios de día, ya que de noche hay otros horarios
            for (int i = 0; i < m_TrainList.Length; i++)
            {
                for (int j = 0; j < m_TrainList[i].StartingHourDay.Length; j++)
                {
                    if (GameManager.Instance.Hour.Equals(m_TrainList[i].StartingHourDay[j]) && GameManager.Instance.Minute.Equals(m_TrainList[i].StartingMinutesDay[j]))
                    {
                        if (!m_TrainList[i].isActive)  //Si no está activo, arrancamos el tren ya que es su hora.
                        {
                            m_TrainList[i].isActive = true;
                            m_TrainList[i].CanMove = true;
                            m_TrainList[i].SetAgentSpeed();
                            m_TrainList[i].SetAgentZero();
                            break;
                        }

                    }
                }
            }
        }
        else //Si en cambio es de noche
        {
            //Usamos los horarios de noche, ya que de día hay otros horarios
            for (int i = 0; i < m_TrainList.Length; i++)
            {
                for (int j = 0; j < m_TrainList[i].StartingHourNight.Length; j++)
                {
                    if (GameManager.Instance.Hour.Equals(m_TrainList[i].StartingHourNight[j]) && GameManager.Instance.Minute.Equals(m_TrainList[i].StartingMinutesNight[j]))
                    {
                        if (!m_TrainList[i].isActive) //Si no está activo, arrancamos el tren ya que es su hora.
                        {
                            m_TrainList[i].isActive = true;
                            m_TrainList[i].CanMove = true;
                            m_TrainList[i].SetAgentSpeed();
                            m_TrainList[i].SetAgentZero();
                            m_TrainList[i].Light.intensity = 1;
                            break;
                        }

                    }
                }
            }
        }

        // Cambios de cámara. Porlo general van a ir F1: cámara 0: general, F2: 1: tren1, F3: 2: tren2, F4: 3: tren3 

        if (Input.GetKeyDown(KeyCode.F1)) //Esta siempre se reserva para la cámara general
        {
            GameManager.Instance.ChangeCamera(0);
        }

        if (Input.GetKeyDown(KeyCode.F2)) 
        {
            GameManager.Instance.ChangeCamera(1);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            GameManager.Instance.ChangeCamera(2);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            GameManager.Instance.ChangeCamera(3);
        }
    }

    //=================================================================================================================================//
    //                  ** Método que para el tiempo **                                                                                //
    //=================================================================================================================================//
    /// <summary>
    /// Método que para el tiempo. Lo haría en el gamemanager, pero no sé por qué no coge la librería Time.
    /// </summary>
    /// <param name="stop"> Si paramos el tiempo o no. true: si, false no</param>
    public void StopEverything(bool stop)
    {
        if (stop)
            Time.timeScale = 0.0f;
        else
            Time.timeScale = 1.0f;
    }

    //=================================================================================================================================//
    //                  ** Método que acelera el tiempo **                                                                             //
    //=================================================================================================================================//

    /// <summary>
    /// Método que acelera el tiempo.
    /// </summary>
    /// <param name="newTime">Tiempo al que se mueve todo. El tiempo normal es 1.0f</param>
    public void AccelerateWorld(float newTime)
    {
        Time.timeScale = newTime;
    }
}
