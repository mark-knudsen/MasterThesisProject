; ModuleID = 'repl_module'
source_filename = "repl_module"

@df = external global ptr
@str = private unnamed_addr constant [12 x i8] c"Hary cotter\00", align 1

define ptr @main_10() {
entry:
  %df_load = load ptr, ptr @df, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %record_buffer = call ptr @malloc(i64 40)
  %field_mem = call ptr @malloc(i64 8)
  %cast = bitcast ptr %field_mem to ptr
  store i64 1002, ptr %cast, align 4
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %field_mem, ptr %field_ptr, align 8
  %field_ptr1 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr @str, ptr %field_ptr1, align 8
  %field_mem2 = call ptr @malloc(i64 8)
  %cast3 = bitcast ptr %field_mem2 to ptr
  store i64 10, ptr %cast3, align 4
  %field_ptr4 = getelementptr ptr, ptr %record_buffer, i64 2
  store ptr %field_mem2, ptr %field_ptr4, align 8
  %field_mem5 = call ptr @malloc(i64 8)
  %cast6 = bitcast ptr %field_mem5 to ptr
  store i1 false, ptr %cast6, align 1
  %field_ptr7 = getelementptr ptr, ptr %record_buffer, i64 3
  store ptr %field_mem5, ptr %field_ptr7, align 8
  %field_mem8 = call ptr @malloc(i64 8)
  %cast9 = bitcast ptr %field_mem8 to ptr
  store double 1.000000e+06, ptr %cast9, align 8
  %field_ptr10 = getelementptr ptr, ptr %record_buffer, i64 4
  store ptr %field_mem8, ptr %field_ptr10, align 8
  %val_to_ptr = bitcast ptr %record_buffer to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %add_cont

grow:                                             ; preds = %entry
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %entry
  %final_data_ptr = phi ptr [ %data_ptr, %entry ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %val_to_ptr, ptr %target_ptr, align 8
  %next_len = add i64 %len, 1
  store i64 %next_len, ptr %len_ptr, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 4
  %data_ptr11 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %df_load, ptr %data_ptr11, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
