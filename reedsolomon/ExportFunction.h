#ifndef _EXPORT_FUNCTION_H
#define  _EXPORT_FUNCTION_H

#include "pch.h"
#include "reedsolomon.h"

struct ReedSolomonData {
	byte** arrays;
	int* lengths;
	int cnt;
};

static ReedSolomon g_reedSolomon;

void __stdcall ReedSolomon_Init(int dataShards, int parityShards);
void __stdcall ReedSolomon_Encode(intptr_t* result);
void __stdcall ReedSolomon_Reconstruct(intptr_t* result);
void __stdcall ReedSolomon_Create(int* arraySizes, int cnt, intptr_t* result);
void __stdcall ReedSolomon_Release(intptr_t* result);

#endif