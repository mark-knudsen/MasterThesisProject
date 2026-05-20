; ModuleID = 'repl_module'
source_filename = "repl_module"

@df2 = external global ptr
@arr = external global ptr

define ptr @main_6() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %arr_load = load ptr, ptr @arr, align 8
  %add_len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 0
  %add_data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 2
  %add_len = load i64, ptr %add_len_ptr, align 4
  %add_raw_data = load ptr, ptr %add_data_ptr_ptr, align 8
  br label %addrange_loop

addrange_loop:                                    ; preds = %cont, %entry
  %i = phi i64 [ 0, %entry ], [ %next_i, %cont ]
  %loop_cond = icmp ult i64 %i, %add_len
  br i1 %loop_cond, label %addrange_body, label %addrange_after

addrange_body:                                    ; preds = %addrange_loop
  %elem_ptr = getelementptr ptr, ptr %add_raw_data, i64 %i
  %elem = load ptr, ptr %elem_ptr, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %df2_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %df2_load, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %df2_load, i32 0, i32 2
  %len = load i64, ptr %len_ptr, align 8
  %cap = load i64, ptr %cap_ptr, align 8
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

addrange_after:                                   ; preds = %addrange_loop
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %df2_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow:                                             ; preds = %addrange_body
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %realloc, ptr %data_ptr_ptr, align 8
  br label %cont

cont:                                             ; preds = %grow, %addrange_body
  %final_data_ptr = phi ptr [ %data, %addrange_body ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %elem, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr, align 8
  %next_i = add i64 %i, 1
  br label %addrange_loop
}

declare i32 @printf(ptr, ...)

declare ptr @realloc(ptr, i64)

declare noalias ptr @malloc(i64)
