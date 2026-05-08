; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_name_age = type { ptr, i64 }

@z = external global ptr

define ptr @main_4() {
entry:
  %z_load = load ptr, ptr @z, align 8
  %data_ptrs_ptr = getelementptr inbounds nuw %dataframe, ptr %z_load, i32 0, i32 1
  %data_ptrs_array = load ptr, ptr %data_ptrs_ptr, align 8
  %data_ptrs_data = getelementptr inbounds nuw %array, ptr %data_ptrs_array, i32 0, i32 2
  %data_ptrs = load ptr, ptr %data_ptrs_data, align 8
  %row = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr = getelementptr ptr, ptr %data_ptrs, i64 0
  %col_array = load ptr, ptr %col_ptr_ptr, align 8
  %col_data_ptr_ptr = getelementptr inbounds nuw %array, ptr %col_array, i32 0, i32 2
  %col_data_raw = load ptr, ptr %col_data_ptr_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %col_data_raw, i64 0
  %val = load ptr, ptr %elem_ptr, align 8
  %field_ptr = getelementptr inbounds nuw %struct_name_age, ptr %row, i32 0, i32 0
  store ptr %val, ptr %field_ptr, align 8
  %col_ptr_ptr1 = getelementptr ptr, ptr %data_ptrs, i64 1
  %col_array2 = load ptr, ptr %col_ptr_ptr1, align 8
  %col_data_ptr_ptr3 = getelementptr inbounds nuw %array, ptr %col_array2, i32 0, i32 2
  %col_data_raw4 = load ptr, ptr %col_data_ptr_ptr3, align 8
  %elem_ptr5 = getelementptr i64, ptr %col_data_raw4, i64 0
  %val6 = load i64, ptr %elem_ptr5, align 4
  %field_ptr7 = getelementptr inbounds nuw %struct_name_age, ptr %row, i32 0, i32 1
  store i64 %val6, ptr %field_ptr7, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 6, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %row, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i32)
