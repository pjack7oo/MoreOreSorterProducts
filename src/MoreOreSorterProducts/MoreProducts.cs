using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mafi.Collections;
using Mafi.Core.Products;
using Mafi.Core.Vehicles;
using Mafi;
using HarmonyLib;
using Mafi.Core.Buildings.OreSorting;

using System.Reflection.Emit;

namespace ExampleMod
{
    [HarmonyPatch(typeof(VehicleCargo), nameof(VehicleCargo.CanAdd))]
    public class MyPatch
    {
        // private readonly Lyst<KeyValuePair<ProductProto, Quantity>> m_data;
        static TypedFieldInfo<VehicleCargo, Lyst<KeyValuePair<ProductProto, Quantity>>> __m_data;

        static MyPatch()
        {
            __m_data = new("m_data");
        }

        static void Postfix(VehicleCargo __instance, ProductProto product, ref bool __result)
        {
            var this__m_data = __m_data.GetValue(__instance);
            __result = this__m_data.Count == 0
                || __instance.HasProduct(product)
                || (this__m_data.Count != 20 && this__m_data[0].Key.IsMixable && product.IsMixable);
        }
    }

    [HarmonyPatch("Mafi.Core.Vehicles.Excavators.PartialMinedProductTracker", "MaxAllowedQuantityOf")]
    public class MyPatch2
    {
        static FieldInfo __m_minedProducts;
        static FieldInfo __m_maxCapacity;
        static FieldInfo __m_usedCapacity;

        static MyPatch2()
        {
            __m_minedProducts = typeof(Mafi.Core.TruckCaps).Assembly.GetType("Mafi.Core.Vehicles.Excavators.PartialMinedProductTracker")
                .GetField("m_minedProducts", BindingFlags.Instance | BindingFlags.NonPublic);

            __m_maxCapacity = typeof(Mafi.Core.TruckCaps).Assembly.GetType("Mafi.Core.Vehicles.Excavators.PartialMinedProductTracker")
                .GetField("m_maxCapacity", BindingFlags.Instance | BindingFlags.NonPublic);

            __m_usedCapacity = typeof(Mafi.Core.TruckCaps).Assembly.GetType("Mafi.Core.Vehicles.Excavators.PartialMinedProductTracker")
                .GetField("m_usedCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        static void Postfix(object __instance, ProductProto product, ref PartialQuantity __result)
        {
            var m_minedProducts = (LystStruct<KeyValuePair<ProductProto, PartialQuantity>>)__m_minedProducts.GetValue(__instance);
            var m_maxCapacity = (Quantity)__m_maxCapacity.GetValue(__instance);
            var m_usedCapacity = (Quantity)__m_usedCapacity.GetValue(__instance);

            if (m_minedProducts.ContainsKey(product))
            {
                __result = (m_maxCapacity - m_usedCapacity).AsPartial - m_minedProducts.GetValueOrDefault(product).FractionalPart;
                return;
            }

            if (m_minedProducts.Count >= 20)
            {
                __result = PartialQuantity.Zero;
                return;
            }

            __result = (m_maxCapacity - m_usedCapacity).AsPartial;
        }
    }

    [HarmonyPatch(typeof(OreSortingPlant), nameof(OreSortingPlant.AddProductToSort))]
    public class MyPatch3
    {

        static MyPatch3()
        {

        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            
            var codes = new List<CodeInstruction>(instructions);
            //Log.Info("CodeInfo:" + codes + "\n size: " + codes.Count);

            for (var i = 0; i < codes.Count; i++)
            {
                //Log.Info("Code:" + codes[i].opcode + "\nOperand:" + codes[i].operand);

                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    //Log.Info("Found code:" + i);
                   
                    codes[i].opcode = OpCodes.Ldc_I4_S;
                    codes[i].operand = 20;

                }
            }

            return codes.AsEnumerable();
        }
     
    }

    [HarmonyPatch]
    static class Patch
    {
        public static MethodBase TargetMethod()
        {
            Type oreSortingPlantInspector = AccessTools.TypeByName("Mafi.Unity.Ui.Inspectors.OreSortingPlantInspector");
            Type innerClass = AccessTools.FirstInner(oreSortingPlantInspector, (inner) => inner.FullName.Contains("DisplayClass"));
            //Type innerClass = AccessTools.Inner(AccessTools.TypeByName("Mafi.Unity.Ui.Inspectors.OreSortingPlantInspector"), "<>c__DisplayClass0_0");

            //TODO retrieve the name instead of having it hard coded

            //AccessTools.FirstInner(innerClass, (inner) =>
            //{

            //});
            return AccessTools.Method(innerClass, "<.ctor>b__18");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    //Log.Info("Found code:" + i);
                    //index = i;
                    codes[i].opcode = OpCodes.Ldc_I4_S;
                    codes[i].operand = 20;

                }
            }

            return codes.AsEnumerable();
        }
    }

    class TypedFieldInfo<TInstance, TResult>
    {
        FieldInfo _info;
        public TypedFieldInfo(string fieldName) => _info = typeof(TInstance).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        public TResult GetValue(TInstance obj) => (TResult)_info.GetValue(obj);
    }
}
