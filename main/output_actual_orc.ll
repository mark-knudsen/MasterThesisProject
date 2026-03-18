; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = internal global ptr

define ptr @main_2() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %len_ptr = getelementptr i64, ptr %x_load, i64 0
  %length = load i64, ptr %len_ptr, align 8
  %cap_ptr = getelementptr i64, ptr %x_load, i64 1
  %capacity = load i64, ptr %cap_ptr, align 8
  %is_full = icmp eq i64 %length, %capacity
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %entry
  %new_capacity = mul i64 %capacity, 2
  %0 = icmp eq i64 %capacity, 0
  %new_capacity1 = select i1 %0, i64 4, i64 %new_capacity
  %slots = add i64 %new_capacity1, 2
  %total_bytes = mul i64 %slots, 8
  %realloc_array = call ptr @realloc(ptr %x_load, i64 %total_bytes)
  %cap_ptr2 = getelementptr i64, ptr %realloc_array, i64 1
  store i64 %new_capacity1, ptr %cap_ptr2, align 8
  br label %cont

cont:                                             ; preds = %grow, %entry
  %array_ptr_phi = phi ptr [ %x_load, %entry ], [ %realloc_array, %grow ]
  %elem_index = add i64 %length, 2
  %elem_ptr = getelementptr i64, ptr %array_ptr_phi, i64 %elem_index
  store i64 0, ptr %elem_ptr, align 8
  %new_length = add i64 %length, 1
  %len_ptr2 = getelementptr i64, ptr %array_ptr_phi, i64 0
  store i64 %new_length, ptr %len_ptr2, align 8
  store ptr %array_ptr_phi, ptr @x, align 8
  %x_load2 = load ptr, ptr @x, align 8
  %len_ptr3 = getelementptr i64, ptr %x_load2, i32 0
  %length4 = load i64, ptr %len_ptr3, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store i64 %length4, ptr %data_ptr, align 4
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @realloc(ptr, i64)

declare ptr @malloc(i64)
