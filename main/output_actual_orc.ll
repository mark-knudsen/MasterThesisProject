; ModuleID = 'repl_module'
source_filename = "repl_module"

@df = external global ptr

define ptr @main_16() {
entry:
  %df_load = load ptr, ptr @df, align 8
  %dataPointersData_field = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %df_load, i32 0, i32 1
  %dataPointersData = load ptr, ptr %dataPointersData_field, align 8
  %length_ptr = getelementptr i64, ptr %dataPointersData, i64 0
  %length = load i64, ptr %length_ptr, align 8
  %has_columns = icmp ugt i64 %length, 0
  br i1 %has_columns, label %capacity_ok, label %capacity_fail

capacity_ok:                                      ; preds = %entry
  %data_ptr = getelementptr i64, ptr %dataPointersData, i64 2
  %data_ptr1 = load ptr, ptr %data_ptr, align 8
  %first_slot = getelementptr ptr, ptr %data_ptr1, i64 0
  %first_column = load ptr, ptr %first_slot, align 8
  %first_column_array = bitcast ptr %first_column to ptr
  %capacity_ptr = getelementptr i64, ptr %first_column_array, i64 1
  %capacity = load i64, ptr %capacity_ptr, align 8
  %value_mem = call ptr @malloc(i64 8)
  store i64 %capacity, ptr %value_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 1, ptr %tag_ptr, align 8
  %data_ptr2 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr2, align 8
  ret ptr %runtime_obj

capacity_fail:                                    ; preds = %entry
  unreachable
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
