﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vika_Anti_VMP_OOP_3._5_Based.Vika_Recompiler_Structures;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Vika_Anti_VMP.Helpers;

namespace Vika_Anti_VMP_OOP_3._5_Based.Ariphmetics
{
    class And : IOpCode
    {
        public override bool is_opcode(MethodDef md)
        {
            List<OpCode> codes_orig = new List<OpCode>();
            md.Body.SimplifyMacros(null);
           foreach(var instr in md.Body.Instructions)
            {
                codes_orig.Add(instr.OpCode);
            }
            if (codes_orig.Count == pattern.Count)
            {
              for(int i = 0; i < codes_orig.Count; i++)
                {
                    if (codes_orig[i] != pattern[i]) return false;
                }
            }
            else
            {
                return false;
            }
          
                var add_oper = md.Body.Instructions[10].Operand as MethodDef;
            Mutation_Cleaner.clean(add_oper);

            foreach (var instr in add_oper.Body.Instructions)
                {
                    if (instr.OpCode == OpCodes.Shl)
                    {
                   
                        return false;
                    }
                }
            foreach (var instr in add_oper.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.And)
                {

                    return true;
                }
            }

            return false;
        }

        public override int Op_Size()
        {
            return 0;
        }
        public override bool nop_before()
        {
            return false;
        }

        public override bool is_specially()
        {
            return false;
        }
        public override Vika_Instruction restore_instruction(BinaryReader read, Vika_Method md, Vika_Context vika)
        {
            return new Vika_Instruction(OpCodes.And);
        }


        List<OpCode> pattern = new List<OpCode>() {
            OpCodes.Ldarg,
            OpCodes.Call,
            OpCodes.Stloc,
            OpCodes.Ldarg,
            OpCodes.Call,
            OpCodes.Stloc,
            OpCodes.Ldarg,
            OpCodes.Ldarg,
            OpCodes.Ldloc,
            OpCodes.Ldloc,
            OpCodes.Call,
            OpCodes.Call,
            OpCodes.Ret

        };
    }
}
