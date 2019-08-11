#include "ExportFunction.h"

void Wrapper(ReedSolomonData* data, std::vector<row_type>& shards)
{
	for (size_t i = 0; i < data->cnt; i++)
	{
		int vtCnt = data->lengths[i];
		byte* ary = data->arrays[i];

		row_type row(new std::vector<byte>(vtCnt));

		for (size_t j = 0; j < vtCnt; j++)
		{
			(*row)[j] = ary[j];
		}

		shards[i] = row;
	}
}

void Unwrapper(std::vector<row_type>& shards, ReedSolomonData*& data)
{
	int cnt = shards.size();
	int* arraySizes = new int[cnt]();
	for (size_t i = 0; i < cnt; i++)
	{
		arraySizes[i] = shards[i]->size();
	}

	intptr_t ptr = 0;
	ReedSolomon_Create(arraySizes, cnt, &ptr);
	delete[] arraySizes;

	data = (ReedSolomonData*)ptr;

	for (size_t i = 0; i < cnt; i++)
	{
		row_type row = shards[i];
		memcpy(data->arrays[i], row->data(), row->size());
	}
}

void __stdcall ReedSolomon_Init(int dataShards, int parityShards)
{
	g_reedSolomon = ReedSolomon::New(dataShards, parityShards);
}

void __stdcall ReedSolomon_Encode(intptr_t* result)
{
	ReedSolomonData* data = (ReedSolomonData*)(*result);

	std::vector<row_type> shards(data->cnt);
	Wrapper(data, shards);

	ReedSolomon_Release(result);

	//Test
	/*
	for (size_t i = 0; i < shards.size(); i++)
	{
		row_type row = shards[i];

		for (size_t j = 0; j < row->size(); j++)
		{
			(*row)[j] += 1;
		}
	}
	*/
	g_reedSolomon.Encode(shards);

	Unwrapper(shards, data);

	*result = (intptr_t)data;
}

void __stdcall ReedSolomon_Reconstruct(intptr_t* result)
{
	ReedSolomonData* data = (ReedSolomonData*)(*result);

	std::vector<row_type> shards(data->cnt);
	Wrapper(data, shards);

	ReedSolomon_Release(result);

	//Test
	/*
	for (size_t i = 0; i < shards.size(); i++)
	{
		row_type row = shards[i];
		for (size_t j = 0; j < row->size(); j++)
		{
			(*row)[j] -= 1;
		}
	}
	*/
	g_reedSolomon.Reconstruct(shards);

	Unwrapper(shards, data);

	*result = (intptr_t)data;
}

void __stdcall ReedSolomon_Create(int* arraySizes, int cnt, intptr_t* result)
{
	ReedSolomonData* data = new ReedSolomonData();
	data->cnt = cnt;
	data->arrays = new  byte * [cnt]();
	data->lengths = new int[cnt]();

	for (size_t i = 0; i < cnt; i++)
	{
		int size = arraySizes[i];
		data->lengths[i] = size;
		data->arrays[i] = new byte[size]();
	}

	(*result) = (intptr_t)data;
}

void __stdcall ReedSolomon_Release(intptr_t* result)
{
	ReedSolomonData* data = (ReedSolomonData*)(*result);

	for (size_t i = 0; i < data->cnt; i++)
	{
		delete[] data->arrays[i];
	}
	delete[] data->arrays;
	delete[] data->lengths;
	delete data;

	(*result) = 0;
}