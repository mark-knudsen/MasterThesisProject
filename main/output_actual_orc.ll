; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@fmt_array = private unnamed_addr constant [16 x i8] c"Array(len=%ld)\0A\00", align 1
@fmt_array.1 = private unnamed_addr constant [16 x i8] c"Array(len=%ld)\0A\00", align 1

define ptr @main_8() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %arr_ptr = call ptr @malloc(i64 32)
  %len_ptr = getelementptr i64, ptr %arr_ptr, i32 0
  store i64 2, ptr %len_ptr, align 8
  %cap_ptr = getelementptr i64, ptr %arr_ptr, i32 1
  store i64 2, ptr %cap_ptr, align 8
  %idx_0 = getelementptr i64, ptr %arr_ptr, i32 2
  store i64 5, ptr %idx_0, align 8
  %idx_1 = getelementptr i64, ptr %arr_ptr, i32 3
  store i64 6, ptr %idx_1, align 8
  %x_load1 = load ptr, ptr @x, align 8
  %len_ptr2 = getelementptr i64, ptr %x_load1, i32 0
  %length = load i64, ptr %len_ptr2, align 8
  %cap_ptr3 = getelementptr i64, ptr %x_load1, i32 1
  %capacity = load i64, ptr %cap_ptr3, align 8
  %is_full = icmp eq i64 %length, %capacity
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %entry
  %new_capacity = mul i64 %capacity, i64 2
  %slots = add i64 %new_capacity, i64 2
  %total_bytes = mul i64 %slots, i64 8
  %realloc_array = call ptr @realloc(ptr %x_load1, i64 %total_bytes)
  %cap_ptr2 = getelementptr i64, ptr %realloc_array, i64 1
  store i64 %new_capacity, ptr %cap_ptr2, align 8
  br label %cont

cont:                                             ; preds = %grow, %entry
  %array_ptr_phi = phi ptr [ %x_load1, %entry ], [ %realloc_array, %grow ]
  %elem_index = add i64 %length, i64 2
  %elem_ptr = getelementptr i64, ptr %array_ptr_phi, i64 %elem_index
  store i64 5, ptr %elem_ptr, align 8
  %new_length = add i64 %length, i64 1
  store i64 %new_length, ptr %len_ptr2, align 8
  %x_load4 = load ptr, ptr @x, align 8
  %len_ptr5 = getelementptr i64, ptr %x_load4, i32 0
  %length6 = load i64, ptr %len_ptr5, align 8
  %cap_ptr7 = getelementptr i64, ptr %x_load4, i32 1
  %capacity8 = load i64, ptr %cap_ptr7, align 8
  %is_full9 = icmp eq i64 %length6, %capacity8
  br i1 %is_full9, label %grow10, label %cont11

grow10:                                           ; preds = %cont
  %new_capacity12 = mul i64 %capacity8, i64 2
  %slots13 = add i64 %new_capacity12, i64 2
  %total_bytes14 = mul i64 %slots13, i64 8
  %realloc_array15 = call ptr @realloc(ptr %x_load4, i64 %total_bytes14)
  %cap_ptr216 = getelementptr i64, ptr %realloc_array15, i64 1
  store i64 %new_capacity12, ptr %cap_ptr216, align 8
  br label %cont11

cont11:                                           ; preds = %grow10, %cont
  %array_ptr_phi17 = phi ptr [ %x_load4, %cont ], [ %realloc_array15, %grow10 ]
  %elem_index18 = add i64 %length6, i64 2
  %elem_ptr19 = getelementptr i64, ptr %array_ptr_phi17, i64 %elem_index18
  store i64 6, ptr %elem_ptr19, align 8
  %new_length20 = add i64 %length6, i64 1
  store i64 %new_length20, ptr %len_ptr5, align 8
  %arr_len_ptr = bitcast ptr %array_ptr_phi17 to ptr
  %arr_len = load i64, ptr %arr_len_ptr, align 8
  %printcall = call i32 (ptr, ptr, ...) @printf(ptr @fmt_array, i64 %arr_len)
  %arr_len_ptr21 = bitcast ptr %array_ptr_phi17 to ptr
  %arr_len22 = load i64, ptr %arr_len_ptr21, align 8
  %printcall23 = call i32 (ptr, ptr, ...) @printf(ptr @fmt_array.1, i64 %arr_len22)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %array_ptr_phi17, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
