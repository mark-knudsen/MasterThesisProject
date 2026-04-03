; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr

define ptr @main_31() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 2
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
  store ptr bitcast (i64 9 to ptr), ptr %target_ptr, align 8
  %next_len = add i64 %len, 1
  store i64 %next_len, ptr %len_ptr, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr1 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %x_load, ptr %data_ptr1, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @realloc(ptr, i64)

declare ptr @malloc(i64)
