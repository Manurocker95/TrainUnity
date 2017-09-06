using UnityEngine;
using System.Collections;

/// <summary>
/// Semáforos. Cómo funcionan: Cada trigger puede ser de salida o de entrada y cada uno guarda referencia al de salida si es
/// entrada o al de entrada si es salida, junto con el del cruce. En caso de que no haya llegado otro antes al trigger del 
/// cruce, éste le dice al del cruce que hasta que no toque la salida, si llega algún tren, lo pare. Una vez toca el de salida
/// el tren continúa. 
/// Es importante poner estos trigger a distancia suficiente, ya que se mueven con navmesh y requieren un espacio para parar.
/// </summary>

public class TrainLight : MonoBehaviour
{
    /// <summary>
    /// Variables. Algunas se han como Serialize para poner la referencia desde el inspector, o simplemente para controlarlas desde el inspector.
    /// </summary>
    #region variables
    [SerializeField] private Material[] m_TrafficLightMeterials;    // Materiales Rojo y verde del semaforo

    [SerializeField] GameObject m_Signal;                           // Semáforo al que poner en rojo
    [SerializeField] GameObject m_EnterExitReference;               // Referencia al trigger de salida/Entrada
    [SerializeField] GameObject m_CrossTriggerReferences;           // Referencia al trigger del cruce al que parar
    [SerializeField] bool m_Enter;                                  // Comprobador de si es de entrada o salida

    [SerializeField] private GameObject m_goTrainWaiting;           // GameObject que está esperando. Se pone cuando llega uno después de que haya otro antes pasando
    [SerializeField] private bool m_bTrainPassing;                  // Implica que otro tren externo al que ha activado este trigger ha entrado y tenemos que parar temporalmente al nuestro.
    [SerializeField] private bool m_bTrainWaiting;                  // Comprobador de que hay un tren pasando previamente.

    //Properties
    public GameObject TrainWaiting { set { m_goTrainWaiting = value; } get { return m_goTrainWaiting; } }
    public bool IsTrainWaiting { set { m_bTrainWaiting = value; } get { return m_bTrainWaiting; } }
    public bool TrainPassing{ set { m_bTrainPassing = value; } get { return m_bTrainPassing; } }
    #endregion

    //=================================================================================================================================//
    //         ** Método que recibe una colisión con un tren y para si es necesario al que llega. Avisa a los que corresponde**        //
    //=================================================================================================================================//

    /// <summary>
    /// Método que Recibe una colisión de tipo trigger. El objeto que lleva este script tiene un collider con trigger y choca con el tren
    /// que tiene Rigidbody y un BoxCollider
    /// </summary>
    /// <param name="other"> Collider contra el que colisiona (Tren)</param>
    void OnTriggerEnter(Collider other)
    {
        // Si no es un tren, no nos interesa
        if (other.tag != "Train")
        {
            Debug.LogError("No toca un tren");
            return;
        }

        //Si este es un trigger de entrada
        if (m_Enter)
        {
            Debug.Log(other.name+" ha entrado en un trigger.");
            //Y no hay nadie pasando previamente
            if (!m_bTrainPassing)
            {
                m_CrossTriggerReferences.GetComponent<TrainLight>().SetLightMaterial(true);
                m_EnterExitReference.GetComponent<TrainLight>().SetLightMaterial(true);
                Debug.Log("No hay uno previamente");
                //Decimos al de salida que hay un tren pasando
                m_EnterExitReference.GetComponent<TrainLight>().TrainPassing = true;
                m_CrossTriggerReferences.GetComponent<TrainLight>().TrainPassing = true;           
            }
            else //Y hay alguien pasando previamente
            {
                Debug.Log("Hay uno previamente");
                //Paramos el tren que ha tocado este trigger
                other.gameObject.GetComponent<Train>().StopTrain();
                m_bTrainWaiting = true;
                m_goTrainWaiting = other.gameObject;
                m_CrossTriggerReferences.GetComponent<TrainLight>().TrainWaiting = other.gameObject;
            }

            m_bTrainPassing = true;
        }
        else //Si este es un trigger de salida
        {
            // Ya no hay ten pasando
            Debug.Log("Ha salido de un trigger.");
            m_CrossTriggerReferences.GetComponent<TrainLight>().TrainPassing = false;
            m_EnterExitReference.GetComponent<TrainLight>().TrainPassing = false;
            m_bTrainPassing = false;

            // Si había alguien esperando, lo desparamos: ¡Rearrancamos!
            if (m_CrossTriggerReferences.GetComponent<TrainLight>().IsTrainWaiting) 
            {
                Debug.Log("Habia un tren esperando.");
                m_EnterExitReference.GetComponent<TrainLight>().IsTrainWaiting = false;
                m_CrossTriggerReferences.GetComponent<TrainLight>().IsTrainWaiting = false;
                m_CrossTriggerReferences.GetComponent<TrainLight>().TrainWaiting.GetComponent<Train>().BootTrain();
                m_EnterExitReference.GetComponent<TrainLight>().TrainWaiting = null;
                m_CrossTriggerReferences.GetComponent<TrainLight>().TrainWaiting = null;
                m_goTrainWaiting = null;
                m_bTrainWaiting = false;
            }
            m_CrossTriggerReferences.GetComponent<TrainLight>().SetLightMaterial(false);
            m_EnterExitReference.GetComponent<TrainLight>().SetLightMaterial(false);
        }

    }

    public void SetLightMaterial(bool red)
    {
        if (red)
        {
            m_Signal.GetComponent<MeshRenderer>().material = m_TrafficLightMeterials[0];
        }
        else
        {
            m_Signal.GetComponent<MeshRenderer>().material = m_TrafficLightMeterials[1];
        }
    }
}
