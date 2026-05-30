; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr

define ptr @main_17() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 2
  %len = load i64, ptr %len_ptr, align 8
  %cap = load i64, ptr %cap_ptr, align 8
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %entry
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %realloc, ptr %data_ptr_ptr, align 8
  br label %cont

cont:                                             ; preds = %grow, %entry
  %data_phi = phi ptr [ %data, %entry ], [ %realloc, %grow ]
  %slot = getelementptr i64, ptr %data_phi, i64 %len
  store i64 109, ptr %slot, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %x_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @realloc(ptr, i64)

declare noalias ptr @malloc(i64)
