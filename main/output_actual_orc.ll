; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }

@df = external global ptr

define ptr @main_5() {
entry:
  %df_load = load ptr, ptr @df, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %df_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_ptr, align 8
  %rows_header_ptr = bitcast ptr %rows_ptr to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_header_ptr, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_header_ptr, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %result = alloca ptr, align 8
  store ptr null, ptr %result, align 8
  br label %loop

loop:                                             ; preds = %continue, %entry
  %i = phi i64 [ 0, %entry ], [ %next, %continue ]
  %cond = icmp ult i64 %i, %len
  br i1 %cond, label %loop_body, label %end

loop_body:                                        ; preds = %loop
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %i
  %record = load ptr, ptr %elem_ptr, align 8
  %field0_ptr = getelementptr ptr, ptr %record, i64 0
  %field0 = load ptr, ptr %field0_ptr, align 8
  %index_val = load i64, ptr %field0, align 8
  %cmp = icmp eq i64 %index_val, 2
  br i1 %cmp, label %found, label %continue

continue:                                         ; preds = %loop_body
  %next = add i64 %i, 1
  br label %loop

found:                                            ; preds = %loop_body
  store ptr %record, ptr %result, align 8
  br label %end

end:                                              ; preds = %found, %loop
  %result_val = load ptr, ptr %result, align 8
  %ptr_name = getelementptr ptr, ptr %result_val, i64 1
  %val_in_name = load ptr, ptr %ptr_name, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 4, ptr %tag_ptr, align 8
  %data_ptr1 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %val_in_name, ptr %data_ptr1, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
