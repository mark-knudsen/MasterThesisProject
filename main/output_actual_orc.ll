; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr

define ptr @main_6() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %array_cast = bitcast ptr %x_load to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %array_cast, i32 0, i32 0
  %length = load i64, ptr %len_ptr, align 4
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %array_cast, i32 0, i32 1
  %capacity = load i64, ptr %cap_ptr, align 4
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %array_cast, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp eq i64 %length, %capacity
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %entry
  %0 = icmp eq i64 %capacity, 0
  %new_cap = mul i64 %capacity, 2
  %1 = select i1 %0, i64 4, i64 %new_cap
  %new_bytes = mul i64 %1, 8
  %new_data = call ptr @malloc(i64 %new_bytes)
  %copy_bytes = mul i64 %length, 8
  call addrspace(6184193) void <badref>(ptr %new_data, ptr %data_ptr, i64 %copy_bytes, i1 false)
  store ptr %new_data, ptr %data_ptr_ptr, align 8
  store i64 %1, ptr %cap_ptr, align 4
  br label %cont

cont:                                             ; preds = %grow, %entry
  %final_data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr = getelementptr i64, ptr %final_data, i64 %length
  store i64 0, ptr %elem_ptr, align 8
  %new_len = add i64 %length, 1
  store i64 %new_len, ptr %len_ptr, align 4
  %ret_cast = bitcast ptr %array_cast to ptr
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr1 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %ret_cast, ptr %data_ptr1, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
