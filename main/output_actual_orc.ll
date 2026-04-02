; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }

@df = external global ptr

define ptr @main_31() {
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
  %cmp = icmp eq i64 %index_val, 0
  br i1 %cmp, label %found, label %continue

continue:                                         ; preds = %loop_body
  %next = add i64 %i, 1
  br label %loop

found:                                            ; preds = %loop_body
  store ptr %record, ptr %result, align 8
  br label %end

end:                                              ; preds = %found, %loop
  %result_val = load ptr, ptr %result, align 8
  %df_load1 = load ptr, ptr @df, align 8
  %rows_ptr_ptr2 = getelementptr inbounds nuw %dataframe, ptr %df_load1, i32 0, i32 1
  %rows_ptr3 = load ptr, ptr %rows_ptr_ptr2, align 8
  %rows_header_ptr4 = bitcast ptr %rows_ptr3 to ptr
  %len_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_header_ptr4, i32 0, i32 0
  %len6 = load i64, ptr %len_ptr5, align 4
  %data_ptr_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_header_ptr4, i32 0, i32 2
  %data_ptr8 = load ptr, ptr %data_ptr_ptr7, align 8
  %result14 = alloca ptr, align 8
  store ptr null, ptr %result14, align 8
  br label %loop9

loop9:                                            ; preds = %continue11, %end
  %i15 = phi i64 [ 0, %end ], [ %next23, %continue11 ]
  %cond16 = icmp ult i64 %i15, %len6
  br i1 %cond16, label %loop_body10, label %end13

loop_body10:                                      ; preds = %loop9
  %elem_ptr17 = getelementptr ptr, ptr %data_ptr8, i64 %i15
  %record18 = load ptr, ptr %elem_ptr17, align 8
  %field0_ptr19 = getelementptr ptr, ptr %record18, i64 0
  %field020 = load ptr, ptr %field0_ptr19, align 8
  %index_val21 = load i64, ptr %field020, align 8
  %cmp22 = icmp eq i64 %index_val21, 0
  br i1 %cmp22, label %found12, label %continue11

continue11:                                       ; preds = %loop_body10
  %next23 = add i64 %i15, 1
  br label %loop9

found12:                                          ; preds = %loop_body10
  store ptr %record18, ptr %result14, align 8
  br label %end13

end13:                                            ; preds = %found12, %loop9
  %result_val24 = load ptr, ptr %result14, align 8
  %ptr_name = getelementptr ptr, ptr %result_val24, i64 1
  %load_ptr_name = load ptr, ptr %ptr_name, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 6, ptr %tag_ptr, align 2
  %data_ptr25 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %load_ptr_name, ptr %data_ptr25, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
