; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global ptr null

define ptr @main_0() {
entry:
  %arr_ptr = call ptr @malloc(i64 64)
  %len_ptr = getelementptr i64, ptr %arr_ptr, i64 0
  store i64 6, ptr %len_ptr, align 8
  %cap_ptr = getelementptr i64, ptr %arr_ptr, i64 1
  store i64 6, ptr %cap_ptr, align 8
  %elem_0 = getelementptr ptr, ptr %arr_ptr, i64 2
  store i64 1, ptr %elem_0, align 8
  %elem_1 = getelementptr ptr, ptr %arr_ptr, i64 3
  store i64 2, ptr %elem_1, align 8
  %elem_2 = getelementptr ptr, ptr %arr_ptr, i64 4
  store i64 3, ptr %elem_2, align 8
  %elem_3 = getelementptr ptr, ptr %arr_ptr, i64 5
  store i64 6, ptr %elem_3, align 8
  %elem_4 = getelementptr ptr, ptr %arr_ptr, i64 6
  store i64 7, ptr %elem_4, align 8
  %elem_5 = getelementptr ptr, ptr %arr_ptr, i64 7
  store i64 9, ptr %elem_5, align 8
  %len_ptr1 = getelementptr i64, ptr %arr_ptr, i64 0
  %length = load i64, ptr %len_ptr1, align 4
  %0 = add i64 %length, 2
  %1 = mul i64 %0, 8
  %arr_ptr2 = call ptr @malloc(i64 %1)
  %len_ptr3 = getelementptr i64, ptr %arr_ptr2, i64 0
  %cap_ptr4 = getelementptr i64, ptr %arr_ptr2, i64 1
  store i64 %length, ptr %len_ptr3, align 8
  store i64 %length, ptr %cap_ptr4, align 8
  %i = alloca i64, align 8
  store i64 0, ptr %i, align 4
  br label %loop.cond

loop.cond:                                        ; preds = %loop.body, %entry
  %i_val = load i64, ptr %i, align 4
  %cmp = icmp slt i64 %i_val, %length
  br i1 %cmp, label %loop.body, label %loop.end

loop.body:                                        ; preds = %loop.cond
  %2 = add i64 %i_val, 2
  %src_elem = getelementptr i64, ptr %arr_ptr, i64 %2
  %val = load i64, ptr %src_elem, align 4
  %dst_elem = getelementptr i64, ptr %arr_ptr2, i64 %2
  store i64 %val, ptr %dst_elem, align 4
  %3 = add i64 %i_val, 1
  store i64 %3, ptr %i, align 4
  br label %loop.cond

loop.end:                                         ; preds = %loop.cond
  store ptr %arr_ptr2, ptr @x, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
