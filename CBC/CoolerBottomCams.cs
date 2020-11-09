using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using IMOWAAnotations;
using HarmonyLib;
using HarmonyDNet2Fixes;

namespace CBC
{

    public class CoolerBottomCams : MonoBehaviour
    {
        GameObject rootDaTela;
        GameObject telaDaCamera;

        Camera cameraDaNave;//Pegar isso do jogo

        //Facilitar implementação dele
        public bool _renderizarNaTela { get; set; }

        RenderTexture cameraNaTelaTexture;

        Transform shipChair;

        void Start()
        {

            Debug.Log($"Veio do GameObject {gameObject.transform.name}");

            Debug.Log("No Start");
            shipChair = GameObject.Find("ship_chair").transform;

            Debug.Log($"Achou o GameObject {shipChair.name}");


            Debug.Log("Criando rootDaTela");
            rootDaTela = new GameObject("OWBATelaDaCamera");

            rootDaTela.transform.parent = shipChair; //Ele vira parente do ship_chair


            rootDaTela.transform.localPosition = new Vector3(0.006f, 0.178f, 4.473f); // COlocar no meio da tela
            rootDaTela.transform.localRotation = Quaternion.Euler(-37.185f, 0f, 0f); // Rotacionar para ficar paralelo a tela




            Debug.Log("Criando telaDaCamera");
            telaDaCamera = new GameObject("OWBATelaDaCamera");

            MeshFilter meshFilter = telaDaCamera.AddComponent<MeshFilter>();

            GameObject planeObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Mesh planeMesh = planeObject.GetComponent<MeshFilter>().sharedMesh;
            Destroy(planeObject);

            meshFilter.sharedMesh = planeMesh;

            telaDaCamera.AddComponent<MeshRenderer>();

            telaDaCamera.transform.name = "OWBATelaDaCamera";



            telaDaCamera.transform.rotation = rootDaTela.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
            telaDaCamera.transform.localScale = new Vector3(0.038916f, 0.01f, 0.0414f); // Tamanho da imagem para ela ficar ok
            telaDaCamera.transform.position = rootDaTela.transform.position; // Colocar no meio da tela


            telaDaCamera.renderer.material = new Material(Shader.Find("Diffuse"))
            {
                color = Color.black
            };


            Debug.Log("Pegando a LandingCam");
            cameraDaNave = gameObject.camera;

            Debug.Log($"Achamos a { cameraDaNave.transform.name }");

            Debug.Log("Peraparando a texture da camera");
            cameraNaTelaTexture = new RenderTexture(512, 512, 16);
            cameraNaTelaTexture.Create();



            Debug.Log("Listenners legais");
            _renderizarNaTela = false;
            GlobalMessenger.AddListener("ExitFlightConsole", new Callback(DesligarTela));
            GlobalMessenger.AddListener("ExitLandingView", new Callback(DesligarTela));
            GlobalMessenger.AddListener("EnterLandingView", new Callback(LigarTela));



            Debug.Log(" -- Fim Do Start --");

        }
        
        
        void DesligarTela()
        {
            //Para voltar a renderizar no lugar certo
            cameraDaNave.targetTexture = null;
            telaDaCamera.renderer.material.mainTexture = null;
            telaDaCamera.renderer.material.color = Color.black;


        }

        void LigarTela()
        {
            telaDaCamera.renderer.material.color = Color.white;
        }


        //Nesses dois para ter certeza que não vai sair de lá
        void Update()
        {
            telaDaCamera.transform.position = rootDaTela.transform.position;
            telaDaCamera.transform.rotation = rootDaTela.transform.rotation * Quaternion.Euler(0f, 180f, 0f);



            if (cameraDaNave.enabled == false && _renderizarNaTela)
            {



                cameraNaTelaTexture.Release();
                cameraNaTelaTexture.width = 512;
                cameraNaTelaTexture.height = 512;
                cameraDaNave.targetTexture = cameraNaTelaTexture;
                cameraDaNave.Render();

                telaDaCamera.renderer.material.mainTexture = cameraNaTelaTexture;
            }



        }

        public static bool IsShipCameraOn()
        {
            return GameObject.Find("LandingCam").GetComponent<CoolerBottomCams>()._renderizarNaTela;
        }

        public static void SetTrueRenderizarNaTela()
        {
            GameObject.Find("LandingCam").GetComponent<CoolerBottomCams>()._renderizarNaTela = true;
        }


        [IMOWAModInnit("FlightConsole", "Awake", modName = "CoolerBottomCams")]
        public static void ModInnit(string nomeDoMod)
        {
            GameObject.Find("LandingCam").AddComponent<CoolerBottomCams>();
            Debug.Log($"{nomeDoMod} foi colocado no 'LandingCam'");

            var harmonyInstance = new Harmony("com.ivan.CoolerBottomCams");
            Debug.Log("Criar a instancia do Harmony foi um succeso");
            try
            {
                harmonyInstance.PatchAll();
                Debug.Log("O patching foi um sucesso");
            }
            catch (Exception e)
            {
                Debug.Log($"Ocorreu um erro no patching {e}");
            }
        }

    }


    //Patches (mds alguem me ajuda aaaaaaaaaaaaaah) FUNFOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOAEEEEEEEEEEEEEEEEEEEEEE

    [HarmonyPatch(typeof(FlightConsole))]
    class FlightConsolePatches
    {

        
        [HarmonyTranspiler]
        [HarmonyPatch("ExitFlightConsole")]
        static IEnumerable<CodeInstruction> ExitFlightConsoleTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("ExitFlightConsoleTranspiler começou");

            int index = -1;


            var codes = new List<CodeInstruction>(instructions);


            for (var i = 0; i < codes.Count; i++)
            {
                //Achar por essa sequencia 
                if (codes[i].opcode == OpCodes.Ldfld && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 2].opcode == OpCodes.Brtrue)
                {
                    index = i-1;

                    break;

                }
            }

            //Tirar a sequencia e adicionar o call que queremos
            if (index > -1)
            {
                codes.RemoveRange(index,3);

                codes.Insert(index, HarmonyCodeInstructionsConstructors.Call(typeof(CoolerBottomCams), "IsShipCameraOn"));

                Debug.Log("Criar os CodeInstructions de ExitFlightConsoleTranspiler foi um sucesso");
            }

            foreach (CodeInstruction ci in codes)
            {
                //Debug.Log($"Opcode: {ci.opcode} ; Operand: {ci.operand}");

                yield return ci;
            }
            
        }

        //Só um prefixo mesmo
        [HarmonyPrefix]
        [HarmonyPatch("ExitLandingView")]
        static void ExitLandingViewTranspiler()
        {

            GameObject.Find("LandingCam").GetComponent<CoolerBottomCams>()._renderizarNaTela = false;

        }




        [HarmonyTranspiler]
        [HarmonyPatch("UpdateLandingMode")]
        static IEnumerable<CodeInstruction> UpdateLandingModeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("UpdateLandingModeTranspiler começou");

            List<int> indexesOfLandingCamSeq = new List<int>();

            int index1 = -1;// do _landingCam
            int index2 = -1;// do _playerCam

            var codes = new List<CodeInstruction>(instructions);

            //Achar o primeiro tipo de sequencia e dependedo do que ldfld está chamando, colcoar no index1 ou index2
            for (var i = 0; i < codes.Count; i++)
            {
                if (index1 == -1 || index2 == -1)
                {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 3].opcode == OpCodes.Callvirt)
                    {
                        if (codes[i + 1].LoadsField(AccessTools.Field(typeof(FlightConsole), "_landingCam")))
                            index1 = i;

                        else if (codes[i + 1].LoadsField(AccessTools.Field(typeof(FlightConsole), "_playerCam")))
                            index2 = i;
                    }
                }

                else
                {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldfld &&
                   codes[i + 2].opcode == OpCodes.Callvirt)
                    {
                        if (codes[i + 1].LoadsField(AccessTools.Field(typeof(FlightConsole), "_landingCam")))
                            indexesOfLandingCamSeq.Add(i);

                        else
                            i += 2;

                    }
                }



            }
            if (index1 > -1 && index2>-1 && indexesOfLandingCamSeq.Count>0)
            {
                //Ir de tras para frente nos valores achados para que eles não percam sentido

                for (int i = indexesOfLandingCamSeq.Count - 1; i > -1; i--)
                {
                    codes.RemoveRange(indexesOfLandingCamSeq[i], 3);

                    codes.Insert(indexesOfLandingCamSeq[i], HarmonyCodeInstructionsConstructors.Call(typeof(CoolerBottomCams), "IsShipCameraOn"));
                }

                codes.RemoveRange(index2, 4);

                codes.RemoveRange(index1, 4);
                codes.Insert(index1, HarmonyCodeInstructionsConstructors.Call(typeof(CoolerBottomCams), "SetTrueRenderizarNaTela"));

                Debug.Log("Criar os CodeInstructions de UpdateLandingModeTranspiler foi um sucesso");
            }

            foreach (CodeInstruction ci in codes)
            {
                //Debug.Log($"Opcode: {ci.opcode} ; Operand: {ci.operand}");

                yield return ci;
            }

        }


        [HarmonyTranspiler]
        [HarmonyPatch("Update")]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("UpdateTranspiler começou");

            List<int> indexesOfLandingCamSeq = new List<int>();
            
            var codes = new List<CodeInstruction>(instructions);

            //Achar a sequencia e colocar na lista
            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldfld &&
               codes[i + 2].opcode == OpCodes.Callvirt)
                {
                    if (codes[i + 1].LoadsField(AccessTools.Field(typeof(FlightConsole), "_landingCam")))
                        indexesOfLandingCamSeq.Add(i);

                    else
                        i += 2;

                }
            }

            if ( indexesOfLandingCamSeq.Count > 0)
            {
                //Ir tirando e colocando de tras para frente na lista para uqe os valores adquiridos nela não fiquem atrasados

                for (int i = indexesOfLandingCamSeq.Count - 1; i > -1; i--)
                {
                    codes.RemoveRange(indexesOfLandingCamSeq[i], 3);

                    codes.Insert(indexesOfLandingCamSeq[i], HarmonyCodeInstructionsConstructors.Call(typeof(CoolerBottomCams), "IsShipCameraOn"));
                }

                Debug.Log("Criar os CodeInstructions de UpdateTranspiler foi um sucesso");
            }

            foreach (CodeInstruction ci in codes)
            {
                //Debug.Log($"Opcode: {ci.opcode} ; Operand: {ci.operand}");

                yield return ci;
            }

        }

    }
}

