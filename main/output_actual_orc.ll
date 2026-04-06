; ModuleID = 'repl_module'
source_filename = "repl_module"

@df = external global ptr

define ptr @main_5() {
entry:
  %df_load = load ptr, ptr @df, align 8
  %df_rows_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 1
  %rows_array_header = load ptr, ptr %df_rows_ptr, align 8
  %found_offset = alloca i64, align 8
  store i64 -1, ptr %found_offset, align 4
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_header, i32 0, i32 0
  %len = load i64, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_header, i32 0, i32 2
  %data_ptr = load ptr, ptr %1, align 8
  %i = alloca i64, align 8
  store i64 0, ptr %i, align 4
  br label %search.cond

search.cond:                                      ; preds = %search.next, %entry
  %2 = load i64, ptr %i, align 4
  %3 = icmp ult i64 %2, %len
  br i1 %3, label %search.body, label %search.end

search.body:                                      ; preds = %search.cond
  %4 = getelementptr ptr, ptr %data_ptr, i64 %2
  %5 = load ptr, ptr %4, align 8
  %6 = getelementptr ptr, ptr %5, i64 0
  %7 = load ptr, ptr %6, align 8
  %row_id = load i64, ptr %7, align 4
  %8 = icmp eq i64 %row_id, 2
  br i1 %8, label %search.found, label %search.next

search.next:                                      ; preds = %search.body
  %9 = add i64 %2, 1
  store i64 %9, ptr %i, align 4
  br label %search.cond

search.found:                                     ; preds = %search.body
  store i64 %2, ptr %found_offset, align 4
  br label %search.end

search.end:                                       ; preds = %search.found, %search.cond
  %10 = load i64, ptr %found_offset, align 4
  %11 = icmp ne i64 %10, -1
  br i1 %11, label %do_remove, label %skip_remove

do_remove:                                        ; preds = %search.end
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_header, i32 0, i32 0
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_header, i32 0, i32 2
  %len1 = load i64, ptr %len_ptr, align 4
  %data_ptr2 = load ptr, ptr %data_ptr_ptr, align 8
  %dst = getelementptr ptr, ptr %data_ptr2, i64 %10
  %next_idx = add i64 %10, 1
  %src = getelementptr ptr, ptr %data_ptr2, i64 %next_idx
  %12 = sub i64 %len1, %10
  %move_count = sub i64 %12, 1
  %13 = icmp sgt i64 %move_count, 0
  br i1 %13, label %do_move, label %remove_end

skip_remove:                                      ; preds = %remove_end, %search.end
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr3 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %df_load, ptr %data_ptr3, align 8
  ret ptr %runtime_obj

do_move:                                          ; preds = %do_remove
  %bytes = mul i64 %move_count, 8
  call void @memmove(ptr %dst, ptr %src, i64 %bytes)
  br label %remove_end

remove_end:                                       ; preds = %do_move, %do_remove
  %new_len = sub i64 %len1, 1
  store i64 %new_len, ptr %len_ptr, align 4
  br label %skip_remove
}

declare i32 @printf(ptr, ...)

declare void @memmove(ptr, ptr, i64)

declare ptr @malloc(i64)
