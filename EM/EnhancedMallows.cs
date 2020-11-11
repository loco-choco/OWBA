using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using HarmonyDNet2Fixes;
using IMOWAAnotations;

namespace EM
{
    public class EnhancedMallows
    {
        public static void DefaultMallowScale(Renderer mallowRender) 
        {
            mallowRender.transform.localScale = new Vector3(1f, 1f, 1f);
        }



        public static void ChangeMallowScale(Renderer mallowRender, float fatorDeReducao)
        {
            mallowRender.transform.localScale = new Vector3(4 - 4*fatorDeReducao, 4 - 4 * fatorDeReducao, 4 - 4 * fatorDeReducao);
        }

        public static void Teste(float a)
        {
            Debug.Log($"Valor de _toastLevel : {a}");
        }

        [IMOWAModInnit("PlayerBody", "Awake", modName = "Enhanced Mallows")]
        public static void ModInnit(string nomeDoMod)
        {
            Debug.Log("Começou o ModInnit");
            var harmonyInstance = new Harmony("com.ivan.EnchacedMallows");
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


    [HarmonyPatch(typeof(Marshmallow))]
    [HarmonyPatch("Update")]
    class Marshmallow_UpdatePatch //Marshmallow_Update
    {

        /* static public AccessTools.FieldRef<Marshmallow, Renderer> mallowRenderer =
            AccessTools.FieldRefAccess<Marshmallow, Renderer>("_mallowRenderer");

        static public AccessTools.FieldRef<Marshmallow, float> toastLevel =
            AccessTools.FieldRefAccess<Marshmallow, float>("_toastLevel"); */


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("Transpiler de Marshmallow_UpdatePatch começou");

            int index = -1;


            var codes = new List<CodeInstruction>(instructions);


            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i + 1].opcode == OpCodes.Add && codes[i + 2].opcode == OpCodes.Stfld)//ldc.r4
                {
                    index = i + 3;

                    break;

                }
            }
            if (index > -1)
            {

                codes.Insert(index, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index + 1, HarmonyCodeInstructionsConstructors.LoadField(typeof(Marshmallow), "_mallowRenderer"));
                codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index + 3, HarmonyCodeInstructionsConstructors.LoadField(typeof(Marshmallow), "_toastLevel"));
                codes.Insert(index + 4, HarmonyCodeInstructionsConstructors.Call(typeof(EnhancedMallows), "ChangeMallowScale", new Type[] { typeof(Renderer), typeof(float) }));

                Debug.Log("Criar os CodeInstructions de Marshmallow_UpdatePatch foi um sucesso");
            }

            foreach (CodeInstruction ci in codes)
            {
                //Debug.Log($"Opcode: {ci.opcode} ; Operand: {ci.operand}");

                yield return ci;
            }



        }

    }


    [HarmonyPatch(typeof(Marshmallow))]
    [HarmonyPatch("ResetMarshmallow")]
    class Marshmallow_ResetMarshmallowPatch //Marshmallow_ResetMarshmallow
    {


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("Transpiler de Marshmallow_ResetMarshmallowPatch começou");


            var codes = new List<CodeInstruction>(instructions);

            int index1 = -1;
            int index2 = codes.Count - 1;

            //Achar aonde colocar as instruções
            for (var i = 0; i < index2 + 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ret)
                {

                    index1 = i - 1;
                    break;

                }
            }
            


            //Se achado colocar elas lá
            if (index1 > -1 && index2 > -1)
            {
                codes.Insert(index1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index1 + 1, HarmonyCodeInstructionsConstructors.LoadField(typeof(Marshmallow), "_mallowRenderer"));
                codes.Insert(index1 + 2, HarmonyCodeInstructionsConstructors.Call(typeof(EnhancedMallows), "DefaultMallowScale", new Type[] { typeof(Renderer) }));

                codes.Insert(index2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index2 + 1, HarmonyCodeInstructionsConstructors.LoadField(typeof(Marshmallow), "_mallowRenderer"));
                codes.Insert(index2 + 2, HarmonyCodeInstructionsConstructors.Call(typeof(EnhancedMallows), "DefaultMallowScale", new Type[] { typeof(Renderer) }));

                Debug.Log("Criar os CodeInstructions de Marshmallow_ResetMarshmallowPatch foi um sucesso");


            }



            foreach (CodeInstruction ci in codes)
            {
                //Debug.Log($"Opcode: {ci.opcode} ; Operand: {ci.operand}");
                yield return ci;
            }



        }
    }
}


    


