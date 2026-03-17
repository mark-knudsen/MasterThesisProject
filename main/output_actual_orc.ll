; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@fmt_int = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_2() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %len_ptr = getelementptr i64, ptr %arr_load, i32 0
  %length = load i64, ptr %len_ptr, align 8
  %cap_ptr = getelementptr i64, ptr %arr_load, i32 1
  %capacity = load i64, ptr %cap_ptr, align 8
  %is_full = icmp eq i64 %length, %capacity
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %entry
  %new_capacity = mul i64 %capacity, i64 2
  %slots = add i64 %new_capacity, i64 2
  %total_bytes = mul i64 %slots, i64 8
  %realloc_array = call ptr @realloc(ptr %arr_load, i64 %total_bytes)
  %cap_ptr2 = getelementptr i64, ptr %realloc_array, i64 1
  store i64 %new_capacity, ptr %cap_ptr2, align 8
  br label %cont

cont:                                             ; preds = %grow, %entry
  %array_ptr_phi = phi ptr [ %arr_load, %entry ], [ %realloc_array, %grow ]
  %elem_index = add i64 %length, i64 2
  %elem_ptr = getelementptr i64, ptr %array_ptr_phi, i64 %elem_index
  store i64 34, ptr %elem_ptr, align 8
  %new_length = add i64 %length, i64 1
  store i64 %new_length, ptr %len_ptr, align 8
  %printcall = call i32 (ptr, ptr, ...) @printf(ptr @fmt_int, ptr %array_ptr_phi)
  %int_mem = call ptr @malloc(i64 8)
  store ptr %array_ptr_phi, ptr %int_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %int_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @realloc(ptr, i64)

declare ptr @malloc(i64)
